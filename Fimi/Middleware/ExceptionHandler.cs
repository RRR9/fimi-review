using Fimi.Models;
using System.Text.Json;

namespace Fimi.Middleware
{
    public class ExceptionHandler
    {
        private readonly RequestDelegate _next;
        private readonly LogManager _log;

        public ExceptionHandler(RequestDelegate next)
        {
            _next = next;
            _log = new LogManager(Utils.LogPath);
        }

        public async Task Invoke(HttpContext httpContext)
        {
            try
            {
                await _next(httpContext);
            }
            catch(Exception ex)
            {
                _log.Error(ex);
                
                httpContext.Response.Clear();
                var r = new ErrorResponse()
                {
                    ResultCode = 3,
                    ResultDesc = "Unknown error",
                };

                if (ex is ShukrMoliyaException)
                {
                    r.ResultDesc = ex.Message;
                }

                var s = JsonSerializer.Serialize(r);

                httpContext.Response.ContentType = "application/json";
                httpContext.Response.StatusCode = 400;
                await httpContext.Response.WriteAsync(s);
            }
        }
    }
}
