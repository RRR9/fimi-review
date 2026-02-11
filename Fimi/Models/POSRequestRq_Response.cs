namespace Fimi.Models
{
    public class POSRequestRqResponse
    {
        public int Response { get; set; }
        public int AuthRespCode { get; set; }
        public required string ApprovalCode { get; set; }
        public int ThisTranId { get; set; }
        public required int ResultCode { get; set; }
        public required string ResultDesc { get; set; }
    }
}
