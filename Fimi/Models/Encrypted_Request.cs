namespace Fimi.Models
{
    public class EncryptedRequest
    {
        public required string EncryptedMessage { get; set; }
        public required string Sign { get; set; }
    }
}
