namespace Fimi.Models
{
    public class P2PRequest
    {
        public required int PlatformId { get; set; }
        public required string PAN { get; set; }
        public required string PAN2 { get; set; }
        public required decimal Amount { get; set; }
        public required string ExternalId { get; set; }
        public required int OperationId { get; set; }
    }
}
