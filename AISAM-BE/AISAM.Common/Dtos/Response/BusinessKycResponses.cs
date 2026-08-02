namespace AISAM.Common.Dtos.Response;

public sealed class BusinessKycVerificationResponse
{
    public string TaxId { get; set; } = string.Empty;
    public string SubmittedLegalBusinessName { get; set; } = string.Empty;
    public string? TaxApiBusinessName { get; set; }
    public string? TaxApiStatus { get; set; }
    public string KycStatus { get; set; } = string.Empty;
    public double SimilarityScore { get; set; }
    public bool IsTaxStatusActive { get; set; }
    public string VerificationLevel { get; set; } = "Level1_TaxApi";
    public string Reason { get; set; } = string.Empty;
}
