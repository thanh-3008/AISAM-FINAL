namespace AISAM.Common.Dtos.Request
{
    public class AuditLogFilterRequest : PaginationRequest
    {
        public string? ActionType { get; set; }
        public string? TargetTable { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public Guid? ActorId { get; set; }
    }
}
