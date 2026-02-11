namespace Fimi.Middleware
{
    public class ReadableHttpBody
    {
        private readonly RequestDelegate _next;

        public ReadableHttpBody(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            var responseBodyStream = new MemoryStream();
            var previousBodyStream = httpContext.Response.Body;
            Exception? exception = null;
            try
            {
                httpContext.Response.Body = responseBodyStream;
                await _next(httpContext);
            }
            catch(Exception ex)
            {
                exception = new Exception(ex.Message, ex);
            }
            finally
            {
                responseBodyStream.Flush();
                responseBodyStream.Seek(0, SeekOrigin.Begin);
                await responseBodyStream.CopyToAsync(previousBodyStream);
                httpContext.Response.Body = previousBodyStream;

                if (exception is not null)
                {
                    throw exception;
                }
            }
        }
    }
}
