using Fimi.Models;
using Newtonsoft.Json;
using System.Text;

namespace Fimi.Middleware
{
    public class MD5
    {
        private readonly RequestDelegate _next;

        public MD5(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            EncryptedRequest? encryptedRequest;
            using (var sr = new StreamReader(httpContext.Request.Body))
            {
                encryptedRequest = JsonConvert.DeserializeObject<EncryptedRequest>(await sr.ReadToEndAsync());
            }

            //httpContext.Request.Body.Position = 0;

            if (encryptedRequest is null)
            {
                throw new Exception("Can not deserialize encrypted message");
            }

            if (encryptedRequest.Sign != Security.MD5(encryptedRequest.EncryptedMessage + Utils.PasswordMD5()))
            {
                throw new Exception("MD5 sign is not correct");
            }

            var messageByte = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(encryptedRequest));
            httpContext.Request.Body = new MemoryStream(messageByte);
            httpContext.Request.ContentLength = messageByte.Length;

            await _next(httpContext);
        }
    }
}
