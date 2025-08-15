using TransparentCloudServerProxy.Managed.Models;
using TransparentCloudServerProxy.WebDashboard.Models;
using TransparentCloudServerProxy.WebDashboard.Services;
using TransparentCloudServerProxy.WebDashboard.Services.Interfaces;

using ProxyConfig = TransparentCloudServerProxy.WebDashboard.Models.ProxyConfig;

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
                .AddSingleton<IProxyConfig, ProxyConfig>()
                .AddSingleton<DashboardConfig>();

            builder.Services
                        .AddSingleton<IProxyService, WindowsProxyService>();

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
