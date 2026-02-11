namespace Fimi.Models
{
    public class GetCardInfoRequest
    {
        public required int PlatformId { get; set; }
        public required string PAN { get; set; }
        public required int OperationId { get; set; }
        public string? ExternalId { get; set; }
    }
}
