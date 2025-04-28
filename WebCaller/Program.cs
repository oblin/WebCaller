using Serilog;
using System.Runtime.InteropServices;

namespace WebCaller
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddCors(options =>
            {
                options.AddDefaultPolicy(policy =>
                {
                    policy.AllowAnyOrigin()
                          .AllowAnyMethod()
                          .AllowAnyHeader();
                });
            });

            // setup serilog
            Log.Logger = new LoggerConfiguration()
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .CreateLogger();

            builder.Host.UseSerilog();

            // Add services to the container.
            builder.Services.AddControllers();
            builder.Services.AddSignalR();


            var app = builder.Build();

            // �B��� Linux �ɡA�w�]�ϥ� nginx �ҥ� Reverse Proxy �Ҧ� 
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                app.UseForwardedHeaders(new ForwardedHeadersOptions
                {
                    ForwardedHeaders = Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedFor
                    | Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedProto
                });
                if (!string.IsNullOrEmpty(builder.Configuration["HttpConfiguration:BasePath"]))
                    app.UsePathBase(builder.Configuration["HttpConfiguration:BasePath"]);
            }

            app.UseHttpsRedirection();
            app.UseCors();

            app.MapControllers();

            app.MapHub<Hubs.ControlHub>("/controlhub");

            app.Run();
        }
    }
}
