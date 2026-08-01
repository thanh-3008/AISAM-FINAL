using System.Net;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using AISAM.Common;
using AISAM.Common.Dtos.Request;
using AISAM.Common.Dtos.Response;
using AISAM.Services.IServices;
using AISAM.Services.Utilities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace AISAM.Services.Service;

public sealed partial class BusinessKycService : IBusinessKycService
{
    private const double AutoApproveThreshold = 0.85;

    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<BusinessKycService> _logger;

    public BusinessKycService(
        HttpClient httpClient,
        IConfiguration configuration,
        ILogger<BusinessKycService> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<GenericResponse<BusinessKycVerificationResponse>> SubmitAsync(
        Guid userId,
        SubmitBusinessKycRequest request,
        CancellationToken cancellationToken = default)
    {
        _ = userId; // Reserved for persisting the KYC record once the KYC table is wired in.

        var taxId = DigitsOnlyRegex().Replace(request.TaxId ?? string.Empty, string.Empty);
        if (taxId.Length is < 8 or > 14)
        {
            return GenericResponse<BusinessKycVerificationResponse>.CreateError(
                "Tax ID is invalid.",
                HttpStatusCode.BadRequest,
                "INVALID_TAX_ID");
        }

        var taxLookup = await LookupTaxAsync(taxId, cancellationToken);
        if (taxLookup is null || string.IsNullOrWhiteSpace(taxLookup.Name))
        {
            return GenericResponse<BusinessKycVerificationResponse>.CreateSuccess(
                new BusinessKycVerificationResponse
                {
                    TaxId = taxId,
                    SubmittedLegalBusinessName = request.LegalBusinessName,
                    KycStatus = "Pending_Review",
                    SimilarityScore = 0,
                    IsTaxStatusActive = false,
                    Reason = "Tax API did not return a matching business record."
                },
                "Business KYC requires admin review.");
        }

        var similarity = BusinessNameNormalizer.CalculateSimilarity(request.LegalBusinessName, taxLookup.Name);
        var isActive = IsActiveStatus(taxLookup.Status);
        var shouldAutoApprove = similarity >= AutoApproveThreshold && isActive;

        var response = new BusinessKycVerificationResponse
        {
            TaxId = taxId,
            SubmittedLegalBusinessName = request.LegalBusinessName,
            TaxApiBusinessName = taxLookup.Name,
            TaxApiStatus = taxLookup.Status,
            KycStatus = shouldAutoApprove ? "Verified" : "Pending_Review",
            SimilarityScore = Math.Round(similarity, 4),
            IsTaxStatusActive = isActive,
            Reason = shouldAutoApprove
                ? "Tax ID is active and legal business name similarity is above 85%."
                : BuildReviewReason(similarity, isActive)
        };

        return GenericResponse<BusinessKycVerificationResponse>.CreateSuccess(
            response,
            shouldAutoApprove ? "Business KYC verified." : "Business KYC requires admin review.");
    }

    private async Task<TaxLookupResult?> LookupTaxAsync(string taxId, CancellationToken cancellationToken)
    {
        var endpointTemplate = _configuration["TaxLookup:EndpointTemplate"]
            ?? Environment.GetEnvironmentVariable("TAX_LOOKUP_ENDPOINT_TEMPLATE")
            ?? "https://api.vietqr.io/v2/business/{tax_id}";

        var endpoint = endpointTemplate
            .Replace("{tax_id}", Uri.EscapeDataString(taxId), StringComparison.OrdinalIgnoreCase)
            .Replace("{id}", Uri.EscapeDataString(taxId), StringComparison.OrdinalIgnoreCase);

        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, endpoint);
            request.Headers.UserAgent.ParseAdd("AISAM-KYC/1.0");
            request.Headers.Accept.ParseAdd("application/json");

            using var response = await _httpClient.SendAsync(request, cancellationToken);
            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Tax lookup failed for {TaxId}. Status: {StatusCode}", taxId, response.StatusCode);
                return null;
            }

            var payload = await response.Content.ReadFromJsonAsync<VietQrBusinessResponse>(cancellationToken: cancellationToken);
            if (payload?.Data is null || !IsSuccessfulApiCode(payload.Code))
            {
                return null;
            }

            return new TaxLookupResult(payload.Data.Name, payload.Data.Status);
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            _logger.LogWarning("Tax lookup timed out for {TaxId}", taxId);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Tax lookup failed for {TaxId}", taxId);
            return null;
        }
    }

    private static bool IsSuccessfulApiCode(string? code)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return true;
        }

        return code is "00" or "0" or "200";
    }

    private static bool IsActiveStatus(string? status)
    {
        if (string.IsNullOrWhiteSpace(status))
        {
            // VietQR's public business endpoint may omit status. If data exists, treat it as active for Level 1.
            return true;
        }

        var normalized = BusinessNameNormalizer.NormalizeBusinessName(status);
        return normalized.Contains("dang hoat dong", StringComparison.Ordinal) ||
               normalized.Contains("active", StringComparison.Ordinal) ||
               normalized.Contains("hoat dong", StringComparison.Ordinal);
    }

    private static string BuildReviewReason(double similarity, bool isActive)
    {
        if (!isActive)
        {
            return "Tax API returned an inactive business status.";
        }

        return $"Legal business name similarity is below 85%. Current score: {similarity:P0}.";
    }

    private sealed record TaxLookupResult(string? Name, string? Status);

    private sealed class VietQrBusinessResponse
    {
        [JsonPropertyName("code")]
        public string? Code { get; set; }

        [JsonPropertyName("data")]
        public VietQrBusinessData? Data { get; set; }
    }

    private sealed class VietQrBusinessData
    {
        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("status")]
        public string? Status { get; set; }
    }

    [GeneratedRegex("\\D", RegexOptions.Compiled)]
    private static partial Regex DigitsOnlyRegex();
}
