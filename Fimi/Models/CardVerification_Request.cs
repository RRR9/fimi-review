namespace Fimi.Models
{
    public class CardVerificationRequest
    {
        public required int PlatformId { get; set; }
        public required string PAN { get; set; }
        public required string ExpDate { get; set; }
        public required string CVV2 { get; set;}
        public string? ExternalId { get; set; }
        public required int OperationId { get; set;}
    }
}
