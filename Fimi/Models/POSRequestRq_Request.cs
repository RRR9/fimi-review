namespace Fimi.Models
{
    public class POSRequestRqRequest
    {
        public required int PlatformId { get; set; }
        public required string PAN { get; set; }
        public decimal Amount { get; set; }
        public required string CVV2 { get; set; }
        public required string ExpDate { get; set; }
        public required int OperationId { get; set; }
        public required string ExternalId { get; set; }
    }
}
