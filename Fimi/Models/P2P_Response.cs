namespace Fimi.Models
{
    public class P2PResponse
    {
        public required int ResultCode { get; set; }
        public required string ResultDesc { get; set; }
        public required string Response { get; set; }
        public required double AvailBalance { get; set; }
        public required string AuthRespCode { get; set; }
    }
}
