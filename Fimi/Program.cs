using Fimi.Middleware;

namespace Fimi
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle

            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            builder.Services.AddControllers();
            builder.Services.AddHttpContextAccessor();

            var app = builder.Build();

            app.UseExceptionHandler("/exception");

            app.Map("/exception", app => app.Run(async httpContext =>
            {
                httpContext.Response.Clear();
                httpContext.Response.StatusCode = 404;
                await httpContext.Response.WriteAsync("Internal error in fimi module");
            }));

            // Configure the HTTP request pipeline.
            //if (app.Environment.IsDevelopment())
            //{
            //    app.UseSwagger();
            //    app.UseSwaggerUI();
            //}

            app.UseMiddleware<ExceptionHandler>();
            app.UseMiddleware<ReadableHttpBody>();
            app.UseMiddleware<MD5>();
            app.UseMiddleware<RSA>();

            app.UseRouting();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}"
            );

            app.Run();
        }
    }
}