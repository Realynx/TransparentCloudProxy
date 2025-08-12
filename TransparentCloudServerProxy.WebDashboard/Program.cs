
using TransparentCloudServerProxy.WebDashboard.Models;
using TransparentCloudServerProxy.WebDashboard.Services;

namespace TransparentCloudServerProxy.WebDashboard {
    public class Program {
        public static void Main(string[] args) {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllers();
            builder.Services.AddOpenApi();

            builder.Services
                .AddSingleton<IConfigurationRoot>(builder.Configuration)
                .AddSingleton<ProxyConfig>()
                .AddSingleton<DashboardConfig>();

            builder.Services
                .AddSingleton<IProxyService, ProxyService>();

            var app = builder.Build();

            if (app.Environment.IsDevelopment()) {
                app.MapOpenApi();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();

            app.MapControllers();
            app.UseStaticFiles();

            app.Run();
        }
    }
}
