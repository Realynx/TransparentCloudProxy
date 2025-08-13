using TransparentCloudServerProxy.WebDashboard.Models;
using TransparentCloudServerProxy.WebDashboard.Services;

namespace TransparentCloudServerProxy.WebDashboard {
    public class Program {
        public static void Main(string[] args) {
            var builder = WebApplication.CreateBuilder(args);

            builder.Logging.ClearProviders();
            builder.Logging.AddConsole();

            builder.Services.AddControllers();
            builder.Services.AddOpenApi();

            builder.Services
                .AddSingleton<IConfigurationRoot>(builder.Configuration)
                .AddSingleton<ProxyConfig>()
                .AddSingleton<DashboardConfig>();

            switch (Environment.OSVersion.Platform) {
                case PlatformID.Unix:
                    builder.Services
                        .AddSingleton<IProxyService, RustProxyService>();
                    break;
                default:
                    builder.Services
                        .AddSingleton<IProxyService, WindowsProxyService>();
                    break;
            }

            var app = builder.Build();

            if (app.Environment.IsDevelopment()) {
                app.MapOpenApi();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();

            app.MapFallbackToFile("index.html");

            app.MapControllers();
            app.UseStaticFiles();

            app.Run();
        }
    }
}
