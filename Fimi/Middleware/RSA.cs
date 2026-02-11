using Fimi.Models;
using Newtonsoft.Json.Linq;
using System.Text;
using System.Text.Json;

namespace Fimi.Middleware
{
    public class RSA
    {
        private readonly RequestDelegate _next;

        public RSA(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            EncryptedRequest? encryptedRequest;
            using (var sr = new StreamReader(httpContext.Request.Body))
            {
                encryptedRequest = JsonSerializer.Deserialize<EncryptedRequest>(await sr.ReadToEndAsync());
            }

            //httpContext.Request.Body.Position = 0;

            if (encryptedRequest is null)
            {
                throw new Exception("Can not deserialize encrypted message");
            }

            var decryptedMessage = Security.DecryptRSA(encryptedRequest.EncryptedMessage);

            var jMessage = JObject.Parse(decryptedMessage);
            jMessage["PlatformId"] = 0;

            string s = jMessage.ToString();

            var decryptedMessageByte = Encoding.UTF8.GetBytes(jMessage.ToString());

            httpContext.Request.Body = new MemoryStream(decryptedMessageByte);
            httpContext.Request.ContentLength = decryptedMessageByte.Length;

            await _next(httpContext);
        }
    }
}
