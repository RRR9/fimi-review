namespace Fimi.Models
{
    public class ErrorResponse
    {
        public required int ResultCode { get; set; }
        public required string ResultDesc { get; set; }
    }
}
