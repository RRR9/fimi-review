namespace Fimi.Models
{
    public class OperationCreateResponse
    {
        public int OperationId { get; set; }
        public required int ResultCode { get; set; }
        public required string ResultDesc { get; set; }
    }
}
