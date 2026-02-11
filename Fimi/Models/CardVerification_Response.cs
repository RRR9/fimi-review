namespace Fimi.Models
{
    public class CardVerificationResponse
    {
        public required int ResultCode { get; set; }
        public required string ResultDesc { get; set; }
        public required string Response { get; set; }
        public required string ApprovalCode { get; set; }
        public required string AuthRespCode { get; set; }
        public required string AvailBalance { get; set; }
    }
}
