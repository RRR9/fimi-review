namespace Fimi.Models
{
    public class ReverseTransactionRequest
    {
        public required int PlatformId { get; set; }
        public required string TranId { get; set; }
        public required int OperationId { get; set;}
        public string? ExternalId { get; set;}
    }
}
