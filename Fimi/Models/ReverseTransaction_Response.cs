namespace Fimi.Models
{
    public class ReverseTransactionResponse
    {
        public required int ResultCode { get; set; }
        public required string ResultDesc { get; set; }
        public required string Response { get; set; }
        public required string ThisTranId { get; set; }
    }
}
