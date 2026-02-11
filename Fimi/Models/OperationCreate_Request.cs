namespace Fimi.Models
{
    public class OperationCreateRequest
    {
        public required int PlatformId { get; set; }
        public decimal? Amount { get; set; }
        public required int OperationTypeId { get; set; }
        public string? ExternalId { get; set; }
    }
}
