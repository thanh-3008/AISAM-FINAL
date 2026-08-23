using System.Globalization;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using AISAM.Common;
using AISAM.Common.Dtos;
using AISAM.Common.Models;
using AISAM.Data.Enumeration;
using AISAM.Data.Model;
using AISAM.Repositories.IRepositories;
using AISAM.Services.IServices;

namespace AISAM.Services.Service;

public sealed class AutomationService : IAutomationService
{
    private static readonly HashSet<string> SupportedPlatforms = new(StringComparer.OrdinalIgnoreCase) { "facebook", "instagram", "tiktok" };
    private readonly IAutomationRepository _automationRepository;
    private readonly IBrandRepository _brandRepository;
    private readonly IProductRepository _productRepository;
    private readonly IAutomationCreditService _automationCredits;

    public AutomationService(IAutomationRepository automationRepository, IBrandRepository brandRepository, IProductRepository productRepository, IAutomationCreditService automationCredits)
    {
        _automationRepository = automationRepository;
        _brandRepository = brandRepository;
        _productRepository = productRepository;
        _automationCredits = automationCredits;
    }

    public async Task<GenericResponse<AutomationPlanDto>> CreateAsync(Guid workspaceId, Guid profileId, CreateAutomationPlanRequest request, string? sourceFileName = null, CancellationToken cancellationToken = default)
    {
        if (request.Rows.Count == 0)
            return GenericResponse<AutomationPlanDto>.CreateError("The automation plan must contain at least one row.");

        var plan = new AutomationPlan
        {
            WorkspaceId = workspaceId,
            ProfileId = profileId,
            Name = request.Name.Trim(),
            SourceFileName = sourceFileName,
            Timezone = string.IsNullOrWhiteSpace(request.Timezone) ? "UTC" : request.Timezone.Trim(),
            Status = AutomationPlanStatusEnum.Validating
        };

        var brandNames = request.Rows.Select(r => r.BrandName ?? string.Empty).Where(n => !string.IsNullOrWhiteSpace(n)).Distinct();
        var brandIds = request.Rows.Where(r => r.BrandId.HasValue).Select(r => r.BrandId!.Value).Distinct();
        var workspaceBrands = await _brandRepository.GetByNamesAndIdsAsync(workspaceId, brandNames, brandIds, cancellationToken);

        for (var rowIndex = 0; rowIndex < request.Rows.Count; rowIndex++)
        {
            var row = request.Rows[rowIndex];
            var resolvedBrand = row.BrandId.HasValue && row.BrandId.Value != Guid.Empty
                ? workspaceBrands.FirstOrDefault(brand => brand.Id == row.BrandId.Value)
                : workspaceBrands.FirstOrDefault(brand => string.Equals(brand.Name, row.BrandName?.Trim(), StringComparison.OrdinalIgnoreCase));
            
            if (resolvedBrand is not null)
            {
                row.BrandId = resolvedBrand.Id;
                if (!row.ProductId.HasValue && !string.IsNullOrWhiteSpace(row.ProductName))
                    row.ProductId = resolvedBrand.Products.FirstOrDefault(product => string.Equals(product.Name, row.ProductName.Trim(), StringComparison.OrdinalIgnoreCase))?.Id;
            }
            var platforms = row.Platforms.Where(value => !string.IsNullOrWhiteSpace(value)).Select(NormalizePlatform).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
            if (platforms.Count == 0) platforms.Add("unknown");

            foreach (var platform in platforms)
            {
                var errors = ValidateRow(workspaceId, row, platform, resolvedBrand);
                var type = ParseContentType(row.ContentType);
                var item = new AutomationItem
                {
                    AutomationPlanId = plan.Id,
                    RowIndex = rowIndex + 1,
                    Platform = platform,
                    IdempotencyKey = CreateIdempotencyKey(plan.Id, rowIndex + 1, platform),
                    BrandId = row.BrandId,
                    ProductId = row.ProductId,
                    Topic = row.Topic.Trim(),
                    Objective = NullIfEmpty(row.Objective),
                    RequestedContentType = type,
                    Tone = NullIfEmpty(row.Tone),
                    Cta = NullIfEmpty(row.Cta),
                    Notes = NullIfEmpty(row.Notes),
                    ScheduledAt = NormalizeUtc(row.ScheduledAt),
                    EstimatedCredits = EstimateCredits(type),
                    Status = errors.Count == 0 ? AutomationItemStatusEnum.Pending : AutomationItemStatusEnum.NeedsAttention,
                    ValidationErrors = errors.Count == 0 ? null : JsonSerializer.Serialize(errors),
                    SourceJson = JsonSerializer.Serialize(row)
                };
                plan.Items.Add(item);
            }
        }

        plan.TotalItems = plan.Items.Count;
        plan.ValidItems = plan.Items.Count(item => item.Status == AutomationItemStatusEnum.Pending);
        plan.FailedItems = plan.TotalItems - plan.ValidItems;
        plan.EstimatedCredits = plan.Items.Where(item => item.Status == AutomationItemStatusEnum.Pending).Sum(item => item.EstimatedCredits);
        plan.Status = AutomationPlanStatusEnum.AwaitingConfirmation;
        await _automationRepository.AddAsync(plan, cancellationToken);

        var saved = await _automationRepository.GetByIdAsync(workspaceId, plan.Id, cancellationToken) ?? plan;
        return GenericResponse<AutomationPlanDto>.CreateSuccess(Map(saved), "Automation plan imported and validated.");
    }

    public async Task<GenericResponse<AutomationPlanDto>> ImportCsvAsync(Guid workspaceId, Guid profileId, string name, string timezone, string sourceFileName, Stream stream, CancellationToken cancellationToken = default)
    {
        using var reader = new StreamReader(stream, Encoding.UTF8, true, leaveOpen: true);
        var content = await reader.ReadToEndAsync(cancellationToken);
        if (string.IsNullOrWhiteSpace(content)) return GenericResponse<AutomationPlanDto>.CreateError("CSV is empty.");
        var delimiter = content.Contains(';') && !content.Contains(',') ? ";" : ",";
        
        using var stringReader = new StringReader(content);
        var config = new CsvHelper.Configuration.CsvConfiguration(CultureInfo.InvariantCulture)
        {
            Delimiter = delimiter,
            HasHeaderRecord = true,
            MissingFieldFound = null,
            BadDataFound = null,
            TrimOptions = CsvHelper.Configuration.TrimOptions.Trim,
            IgnoreBlankLines = true
        };
        using var csv = new CsvHelper.CsvReader(stringReader, config);
        
        await csv.ReadAsync();
        csv.ReadHeader();
        var headers = csv.HeaderRecord;
        if (headers == null || headers.Length == 0) return GenericResponse<AutomationPlanDto>.CreateError("CSV must contain a header.");
        
        var rows = new List<AutomationImportRowRequest>();
        while (await csv.ReadAsync())
        {
            var data = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var header in headers)
            {
                if (!string.IsNullOrWhiteSpace(header))
                    data[header] = csv.TryGetField(header, out string? value) ? (value ?? string.Empty) : string.Empty;
            }
            
            var brandIdText = Get(data, "BrandId");
            var productIdText = Get(data, "ProductId");
            var contentTypeText = Get(data, "ContentType");

            rows.Add(new AutomationImportRowRequest
            {
                BrandId = Guid.TryParse(brandIdText, out var brandId) ? brandId : null,
                BrandName = Get(data, "Brand"),
                ProductId = Guid.TryParse(productIdText, out var productId) ? productId : null,
                ProductName = Get(data, "Product"),
                Topic = Get(data, "Topic"),
                Objective = Get(data, "Objective"),
                Platforms = Get(data, "Platforms").Split(new[] { ',', ';', '|' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList(),
                ContentType = string.IsNullOrWhiteSpace(contentTypeText) ? "Auto" : contentTypeText,
                Tone = Get(data, "Tone"),
                Cta = Get(data, "CTA"),
                Notes = Get(data, "Notes"),
                ScheduledAt = ParseScheduledAt(data, timezone)
            });
        }
        
        if (rows.Count == 0) return GenericResponse<AutomationPlanDto>.CreateError("CSV must contain a header and at least one data row.");
        return await CreateAsync(workspaceId, profileId, new CreateAutomationPlanRequest { Name = name, Timezone = timezone, Rows = rows }, sourceFileName, cancellationToken);
    }

    public async Task<GenericResponse<IReadOnlyList<AutomationPlanDto>>> GetAllAsync(Guid workspaceId, CancellationToken cancellationToken = default)
        => GenericResponse<IReadOnlyList<AutomationPlanDto>>.CreateSuccess((await _automationRepository.GetByWorkspaceAsync(workspaceId, cancellationToken)).Select(Map).ToList());

    public async Task<GenericResponse<AutomationPlanDto>> GetByIdAsync(Guid workspaceId, Guid planId, CancellationToken cancellationToken = default)
    {
        var plan = await _automationRepository.GetByIdAsync(workspaceId, planId, cancellationToken);
        return plan is null
            ? GenericResponse<AutomationPlanDto>.CreateError("Automation plan not found.", HttpStatusCode.NotFound)
            : GenericResponse<AutomationPlanDto>.CreateSuccess(Map(plan));
    }

    public async Task<GenericResponse<AutomationPlanDto>> ConfirmAsync(Guid workspaceId, Guid planId, CancellationToken cancellationToken = default)
    {
        var plan = await _automationRepository.GetByIdAsync(workspaceId, planId, cancellationToken);
        if (plan is null) return GenericResponse<AutomationPlanDto>.CreateError("Automation plan not found.", HttpStatusCode.NotFound);
        if (plan.Status != AutomationPlanStatusEnum.AwaitingConfirmation)
            return GenericResponse<AutomationPlanDto>.CreateError("Only plans awaiting confirmation can be confirmed.");
        if (plan.ValidItems == 0) return GenericResponse<AutomationPlanDto>.CreateError("The plan has no valid items to generate.");

        var reservation = await _automationCredits.ReserveAsync(plan.Id, cancellationToken);
        if (!reservation.Success) return GenericResponse<AutomationPlanDto>.CreateError(reservation.Message ?? "Unable to reserve credits.", (HttpStatusCode)reservation.StatusCode, reservation.Error?.ErrorCode);
        plan.ConfirmedAt = DateTime.UtcNow;
        plan.Status = AutomationPlanStatusEnum.Generating;
        plan.UpdatedAt = DateTime.UtcNow;
        await _automationRepository.SaveChangesAsync(cancellationToken);
        return GenericResponse<AutomationPlanDto>.CreateSuccess(Map(plan), "Automation plan confirmed and queued for generation.");
    }

    public async Task<GenericResponse<AutomationPlanDto>> RetryAsync(Guid workspaceId, Guid planId, Guid? itemId = null, CancellationToken cancellationToken = default)
    {
        var plan = await _automationRepository.GetByIdAsync(workspaceId, planId, cancellationToken);
        if (plan is null) return GenericResponse<AutomationPlanDto>.CreateError("Automation plan not found.", HttpStatusCode.NotFound);
        if (plan.Status == AutomationPlanStatusEnum.Cancelled) return GenericResponse<AutomationPlanDto>.CreateError("A cancelled plan cannot be retried.");

        var candidates = plan.Items.Where(item => (!itemId.HasValue || item.Id == itemId.Value) &&
            item.Status == AutomationItemStatusEnum.GenerationFailed).ToList();
        if (candidates.Count == 0) return GenericResponse<AutomationPlanDto>.CreateError("No failed generation item was found to retry.");
        foreach (var item in candidates)
        {
            if (!string.IsNullOrWhiteSpace(item.ValidationErrors)) continue;
            item.Status = AutomationItemStatusEnum.Pending;
            item.LastError = null;
            item.UpdatedAt = DateTime.UtcNow;
        }
        if (!candidates.Any(item => item.Status == AutomationItemStatusEnum.Pending))
            return GenericResponse<AutomationPlanDto>.CreateError("Validation errors must be corrected before retrying.");
        plan.Status = AutomationPlanStatusEnum.Generating;
        plan.ReservedCredits = 0;
        plan.UpdatedAt = DateTime.UtcNow;
        await _automationRepository.SaveChangesAsync(cancellationToken);
        var reservation = await _automationCredits.ReserveAsync(plan.Id, cancellationToken);
        if (!reservation.Success) return GenericResponse<AutomationPlanDto>.CreateError(reservation.Message ?? "Unable to reserve retry credits.", (HttpStatusCode)reservation.StatusCode, reservation.Error?.ErrorCode);
        return GenericResponse<AutomationPlanDto>.CreateSuccess(Map(plan), "Generation retry queued.");
    }

    public async Task<GenericResponse<AutomationPlanDto>> CancelAsync(Guid workspaceId, Guid planId, CancellationToken cancellationToken = default)
    {
        var plan = await _automationRepository.GetByIdAsync(workspaceId, planId, cancellationToken);
        if (plan is null) return GenericResponse<AutomationPlanDto>.CreateError("Automation plan not found.", HttpStatusCode.NotFound);
        if (plan.Status is AutomationPlanStatusEnum.Completed or AutomationPlanStatusEnum.Cancelled)
            return GenericResponse<AutomationPlanDto>.CreateError("This plan can no longer be cancelled.");
        foreach (var item in plan.Items.Where(item => item.Status is AutomationItemStatusEnum.Pending or AutomationItemStatusEnum.GeneratingText or AutomationItemStatusEnum.GeneratingMedia))
        {
            item.Status = AutomationItemStatusEnum.Rejected;
            item.LastError = "Generation cancelled by user.";
            item.UpdatedAt = DateTime.UtcNow;
        }
        plan.Status = AutomationPlanStatusEnum.Cancelled;
        plan.UpdatedAt = DateTime.UtcNow;
        await _automationRepository.SaveChangesAsync(cancellationToken);
        await _automationCredits.ReleaseAsync(plan.Id, cancellationToken);
        return GenericResponse<AutomationPlanDto>.CreateSuccess(Map(plan), "Automation plan cancelled.");
    }

    public async Task<GenericResponse<AutomationPlanDto>> ImportGoogleSheetAsync(Guid workspaceId, Guid profileId, ImportGoogleSheetRequest request, CancellationToken cancellationToken = default)
    {
        if (!Uri.TryCreate(request.Url, UriKind.Absolute, out var source) || source.Scheme != Uri.UriSchemeHttps ||
            !source.Host.Equals("docs.google.com", StringComparison.OrdinalIgnoreCase))
            return GenericResponse<AutomationPlanDto>.CreateError("Only HTTPS Google Sheets URLs from docs.google.com are allowed.");
        var segments = source.AbsolutePath.Split('/', StringSplitOptions.RemoveEmptyEntries);
        var sheetIndex = Array.FindIndex(segments, value => value.Equals("d", StringComparison.OrdinalIgnoreCase));
        if (sheetIndex < 0 || sheetIndex + 1 >= segments.Length)
            return GenericResponse<AutomationPlanDto>.CreateError("Invalid Google Sheets URL.");
        var gid = System.Web.HttpUtility.ParseQueryString(source.Query).Get("gid") ?? "0";
        var exportUrl = $"https://docs.google.com/spreadsheets/d/{Uri.EscapeDataString(segments[sheetIndex + 1])}/export?format=csv&gid={Uri.EscapeDataString(gid)}";
        try
        {
            using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(30) };
            await using var stream = await client.GetStreamAsync(exportUrl, cancellationToken);
            return await ImportCsvAsync(workspaceId, profileId, request.Name, request.Timezone, "google-sheet.csv", stream, cancellationToken);
        }
        catch (Exception exception) when (exception is HttpRequestException or TaskCanceledException)
        {
            return GenericResponse<AutomationPlanDto>.CreateError("Unable to download the Google Sheet. Make sure link sharing is enabled.", HttpStatusCode.BadGateway);
        }
    }

    public async Task<GenericResponse<AutomationPlanDto>> CloneAsync(Guid workspaceId, Guid profileId, Guid planId, CloneAutomationPlanRequest request, CancellationToken cancellationToken = default)
    {
        var source = await _automationRepository.GetByIdAsync(workspaceId, planId, cancellationToken);
        if (source is null) return GenericResponse<AutomationPlanDto>.CreateError("Automation plan not found.", HttpStatusCode.NotFound);
        if (source.Items.Count == 0) return GenericResponse<AutomationPlanDto>.CreateError("The source automation plan is empty and cannot be cloned.", HttpStatusCode.BadRequest);
        var rows = source.Items.Select(item => new AutomationImportRowRequest
        {
            BrandId = item.BrandId, ProductId = item.ProductId, Topic = item.Topic, Objective = item.Objective,
            Platforms = [item.Platform], ContentType = item.RequestedContentType.ToString(), Tone = item.Tone, Cta = item.Cta,
            Notes = item.Notes, ScheduledAt = item.ScheduledAt.AddDays(request.ShiftDays)
        }).ToList();
        var result = await CreateAsync(workspaceId, profileId, new CreateAutomationPlanRequest { Name = request.Name, Timezone = source.Timezone, Rows = rows }, cancellationToken: cancellationToken);
        if (result.Success && result.Data is not null)
        {
            var clone = await _automationRepository.GetByIdAsync(workspaceId, result.Data.Id, cancellationToken);
            if (clone is not null) { clone.TemplateSourcePlanId = source.Id; await _automationRepository.SaveChangesAsync(cancellationToken); result.Data = Map(clone); }
        }
        return result;
    }

    public async Task<GenericResponse<AutomationPlanDto>> SetAutoApproveAsync(Guid workspaceId, Guid planId, bool enabled, CancellationToken cancellationToken = default)
    {
        var plan = await _automationRepository.GetByIdAsync(workspaceId, planId, cancellationToken);
        if (plan is null) return GenericResponse<AutomationPlanDto>.CreateError("Automation plan not found.", HttpStatusCode.NotFound);
        if (plan.Status is not AutomationPlanStatusEnum.AwaitingConfirmation and not AutomationPlanStatusEnum.Generating)
            return GenericResponse<AutomationPlanDto>.CreateError("Auto-approve can only be changed before generation finishes.");
        plan.AutoApprove = enabled; plan.UpdatedAt = DateTime.UtcNow;
        await _automationRepository.SaveChangesAsync(cancellationToken);
        return GenericResponse<AutomationPlanDto>.CreateSuccess(Map(plan), enabled ? "Auto-approve enabled." : "Auto-approve disabled.");
    }

    public async Task<GenericResponse<AutomationPerformanceDto>> GetPerformanceAsync(Guid workspaceId, Guid planId, CancellationToken cancellationToken = default)
    {
        var report = await _automationRepository.GetPerformanceAsync(workspaceId, planId, cancellationToken);
        return report is null ? GenericResponse<AutomationPerformanceDto>.CreateError("Automation plan not found.", HttpStatusCode.NotFound) : GenericResponse<AutomationPerformanceDto>.CreateSuccess(report);
    }

    public async Task<GenericResponse<AutomationPlanDto>> UpdateItemAsync(Guid workspaceId, Guid planId, Guid itemId, UpdateAutomationItemRequest request, CancellationToken cancellationToken = default)
    {
        var plan = await _automationRepository.GetByIdAsync(workspaceId, planId, cancellationToken);
        if (plan is null) return GenericResponse<AutomationPlanDto>.CreateError("Automation plan not found.", HttpStatusCode.NotFound);
        if (plan.Status != AutomationPlanStatusEnum.AwaitingConfirmation)
            return GenericResponse<AutomationPlanDto>.CreateError("Items can only be edited before the plan is confirmed.");
        var item = plan.Items.FirstOrDefault(value => value.Id == itemId);
        if (item is null) return GenericResponse<AutomationPlanDto>.CreateError("Automation item not found.", HttpStatusCode.NotFound);
        var platform = NormalizePlatform(request.Platform);
        if (plan.Items.Any(value => value.Id != item.Id && value.RowIndex == item.RowIndex && value.Platform == platform))
            return GenericResponse<AutomationPlanDto>.CreateError("This row already contains the selected platform.", HttpStatusCode.Conflict);
        if (!TryParseContentType(request.ContentType, out var contentType))
            return GenericResponse<AutomationPlanDto>.CreateError("Unsupported content type.");

        var validationRow = new AutomationImportRowRequest
        {
            BrandId = request.BrandId, ProductId = request.ProductId, Topic = request.Topic, Platforms = [platform],
            ContentType = request.ContentType, Objective = request.Objective, Tone = request.Tone, Cta = request.Cta,
            Notes = request.Notes, ScheduledAt = request.ScheduledAt
        };
        var brand = request.BrandId.HasValue ? await _brandRepository.GetByIdAsync(request.BrandId.Value, cancellationToken) : null;
        var errors = ValidateRow(workspaceId, validationRow, platform, brand);
        item.BrandId = request.BrandId; item.ProductId = request.ProductId; item.Topic = request.Topic.Trim(); item.Platform = platform;
        item.IdempotencyKey = CreateIdempotencyKey(plan.Id, item.RowIndex, platform); item.RequestedContentType = contentType;
        item.Objective = NullIfEmpty(request.Objective); item.Tone = NullIfEmpty(request.Tone); item.Cta = NullIfEmpty(request.Cta);
        item.Notes = NullIfEmpty(request.Notes); item.ScheduledAt = NormalizeUtc(request.ScheduledAt);
        item.EstimatedCredits = EstimateCredits(contentType); item.ValidationErrors = errors.Count == 0 ? null : JsonSerializer.Serialize(errors);
        item.Status = errors.Count == 0 ? AutomationItemStatusEnum.Pending : AutomationItemStatusEnum.NeedsAttention;
        item.SourceJson = JsonSerializer.Serialize(validationRow); item.LastError = null; item.UpdatedAt = DateTime.UtcNow;
        plan.ValidItems = plan.Items.Count(value => value.Status == AutomationItemStatusEnum.Pending);
        plan.FailedItems = plan.TotalItems - plan.ValidItems;
        plan.EstimatedCredits = plan.Items.Where(value => value.Status == AutomationItemStatusEnum.Pending).Sum(value => value.EstimatedCredits);
        plan.UpdatedAt = DateTime.UtcNow;
        await _automationRepository.SaveChangesAsync(cancellationToken);
        var refreshed = await _automationRepository.GetByIdAsync(workspaceId, planId, cancellationToken) ?? plan;
        return GenericResponse<AutomationPlanDto>.CreateSuccess(Map(refreshed), errors.Count == 0 ? "Automation item updated and validated." : "Automation item updated but still needs attention.");
    }

    private static List<AutomationValidationError> ValidateRow(Guid workspaceId, AutomationImportRowRequest row, string platform, Brand? brand)
    {
        var errors = new List<AutomationValidationError>();
        if (brand is null || brand.WorkspaceId != workspaceId || brand.IsDeleted) errors.Add(new AutomationValidationError { Code = "BRAND_NOT_FOUND", Field = "Brand", Message = "Brand does not exist in the active workspace." });
        
        if (row.ProductId.HasValue)
        {
            var product = brand?.Products?.FirstOrDefault(p => p.Id == row.ProductId.Value && !p.IsDeleted);
            if (product is null) errors.Add(new AutomationValidationError { Code = "PRODUCT_NOT_FOUND", Field = "Product", Message = "Product does not exist or does not belong to the selected brand." });
        }
        else if (!string.IsNullOrWhiteSpace(row.ProductName)) errors.Add(new AutomationValidationError { Code = "PRODUCT_NOT_FOUND", Field = "Product", Message = "Product was not found in the selected brand." });
        
        if (string.IsNullOrWhiteSpace(row.Topic)) errors.Add(new AutomationValidationError { Code = "EMPTY_REQUIRED_FIELD", Field = "Topic", Message = "Topic is required." });
        if (!SupportedPlatforms.Contains(platform)) errors.Add(new AutomationValidationError { Code = "INVALID_PLATFORM", Field = "Platform", Message = $"Unsupported platform: {platform}." });
        
        if (!TryParseContentType(row.ContentType, out var type)) errors.Add(new AutomationValidationError { Code = "INVALID_CONTENT_TYPE", Field = "ContentType", Message = $"Unsupported content type: {row.ContentType}." });
        else if (string.Equals(platform, "tiktok", StringComparison.OrdinalIgnoreCase) && type is not AutomationContentTypeEnum.Video and not AutomationContentTypeEnum.Auto)
            errors.Add(new AutomationValidationError { Code = "INVALID_CONTENT_TYPE", Field = "ContentType", Message = "TikTok requires Video or Auto content type." });
            
        if (row.ScheduledAt == default || NormalizeUtc(row.ScheduledAt) <= DateTime.UtcNow) errors.Add(new AutomationValidationError { Code = "SCHEDULE_IN_PAST", Field = "ScheduledAt", Message = "Date and Time must form a valid future date and time." });
        return errors;
    }

    private static AutomationPlanDto Map(AutomationPlan plan) => new()
    {
        Id = plan.Id, Name = plan.Name, SourceFileName = plan.SourceFileName, Timezone = plan.Timezone, Status = plan.Status.ToString(),
        TotalItems = plan.TotalItems, ValidItems = plan.ValidItems, FailedItems = plan.FailedItems,
        EstimatedCredits = plan.EstimatedCredits, ReservedCredits = plan.ReservedCredits, UsedCredits = plan.UsedCredits, ReleasedCredits = plan.ReleasedCredits,
        AutoApprove = plan.AutoApprove, TemplateSourcePlanId = plan.TemplateSourcePlanId,
        CreatedAt = plan.CreatedAt, ConfirmedAt = plan.ConfirmedAt,
        Items = plan.Items.Select(item => new AutomationItemDto
        {
            Id = item.Id, RowIndex = item.RowIndex, Platform = item.Platform, BrandId = item.BrandId, BrandName = item.Brand?.Name ?? string.Empty,
            ProductId = item.ProductId, ContentId = item.ContentId, ContentCalendarId = item.ContentCalendarId, Topic = item.Topic, Objective = item.Objective,
            ContentType = item.RequestedContentType.ToString(), Tone = item.Tone, Cta = item.Cta, Notes = item.Notes,
            ScheduledAt = item.ScheduledAt, Status = item.Status.ToString(), EstimatedCredits = item.EstimatedCredits,
            UsedCredits = item.UsedCredits, GenerationAttemptCount = item.GenerationAttemptCount, LastError = item.LastError,
            GeneratedText = item.Content?.TextContent, GeneratedImageUrl = FirstImageUrl(item.Content?.ImageUrl),
            GeneratedVideoUrl = item.Content?.VideoUrl, VideoProvider = item.VideoProvider,
            ValidationErrors = DeserializeErrors(item.ValidationErrors)
        }).ToList()
    };

    private static IReadOnlyList<AutomationValidationError> DeserializeErrors(string? json)
    {
        if (string.IsNullOrWhiteSpace(json)) return Array.Empty<AutomationValidationError>();
        try { return JsonSerializer.Deserialize<List<AutomationValidationError>>(json) ?? new List<AutomationValidationError>(); }
        catch (JsonException) { return new[] { new AutomationValidationError { Code = "UNKNOWN_ERROR", Field = "Unknown", Message = json ?? "Unknown error" } }; }
    }

    private static string? FirstImageUrl(string? json)
    {
        if (string.IsNullOrWhiteSpace(json)) return null;
        try { return JsonSerializer.Deserialize<List<string>>(json)?.FirstOrDefault(); }
        catch (JsonException) { return null; }
    }

    private static string CreateIdempotencyKey(Guid planId, int rowIndex, string platform)
        => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes($"{planId:N}:{rowIndex}:{platform.ToLowerInvariant()}"))).ToLowerInvariant();
    private static int EstimateCredits(AutomationContentTypeEnum type) => type switch { AutomationContentTypeEnum.Image => 6, AutomationContentTypeEnum.Video => 21, AutomationContentTypeEnum.Auto => 21, _ => 1 };
    private static string NormalizePlatform(string value) => value.Trim().ToLowerInvariant() switch { "fb" => "facebook", "ig" => "instagram", "tik tok" => "tiktok", var normalized => normalized };
    private static DateTime NormalizeUtc(DateTime value) => value.Kind switch { DateTimeKind.Utc => value, DateTimeKind.Local => value.ToUniversalTime(), _ => DateTime.SpecifyKind(value, DateTimeKind.Local).ToUniversalTime() };
    private static bool TryParseContentType(string? value, out AutomationContentTypeEnum type) => Enum.TryParse(value?.Trim(), true, out type);
    private static AutomationContentTypeEnum ParseContentType(string? value) => TryParseContentType(value, out var type) ? type : AutomationContentTypeEnum.Auto;
    private static string? NullIfEmpty(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    private static string Get(IReadOnlyDictionary<string, string> data, string key, string fallback = "") => data.TryGetValue(key, out var value) ? value.Trim() : fallback;

    private static DateTime ParseScheduledAt(IReadOnlyDictionary<string, string> data, string timezone)
    {
        var legacy = Get(data, "ScheduledAt");
        if (!string.IsNullOrWhiteSpace(legacy) && DateTimeOffset.TryParse(legacy, CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces, out var offset))
            return offset.UtcDateTime;

        var dateText = Get(data, "Date");
        var timeText = Get(data, "Time");
        var formats = new[] { "yyyy-MM-dd HH:mm", "dd/MM/yyyy HH:mm", "d/M/yyyy H:mm", "MM/dd/yyyy HH:mm" };
        if (!DateTime.TryParseExact($"{dateText} {timeText}", formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out var local))
            return default;
        try
        {
            var zone = ResolveTimezone(timezone);
            return TimeZoneInfo.ConvertTimeToUtc(DateTime.SpecifyKind(local, DateTimeKind.Unspecified), zone);
        }
        catch (TimeZoneNotFoundException) { return default; }
        catch (InvalidTimeZoneException) { return default; }
        catch (ArgumentException) { return default; }
    }

    private static TimeZoneInfo ResolveTimezone(string? timezone)
    {
        var id = string.IsNullOrWhiteSpace(timezone) ? "UTC" : timezone.Trim();
        try { return TimeZoneInfo.FindSystemTimeZoneById(id); }
        catch (TimeZoneNotFoundException) when (id is "Asia/Ho_Chi_Minh" or "Asia/Bangkok")
        {
            return TimeZoneInfo.FindSystemTimeZoneById(OperatingSystem.IsWindows() ? "SE Asia Standard Time" : "Asia/Ho_Chi_Minh");
        }
    }

}
