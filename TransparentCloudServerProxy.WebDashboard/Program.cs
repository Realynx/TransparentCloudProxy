using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;

using TransparentCloudServerProxy.Managed.Models;
using TransparentCloudServerProxy.WebDashboard.Models;
using TransparentCloudServerProxy.WebDashboard.Services;
using TransparentCloudServerProxy.WebDashboard.Services.Interfaces;
using TransparentCloudServerProxy.WebDashboard.SqlDb;

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
                .AddDbContextFactory<WebDashboardDbContext>(options =>
                    options.UseSqlite("Data Source=proxy.db"));

            builder.Services
                .AddSingleton<IConfigurationRoot>(builder.Configuration)
                .AddSingleton<IProxyConfig, ProxyConfig>()
                .AddSingleton<IProxyService, ProxyService>()
                .AddSingleton<DashboardConfig>()
                .AddSingleton<CredentialsService>();

            builder.Services
                .AddHostedService<DefaultUserService>();

            builder.Services
                .AddAuthorization()
                .AddAuthentication("KeyToken")
                .AddScheme<AuthenticationSchemeOptions, CredentialAuthenticationHandler>("KeyToken", options => { });

            var app = builder.Build();

            if (app.Environment.IsDevelopment()) {
                app.MapOpenApi();
            }

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapFallbackToFile("index.html");

            app.MapControllers();
            app.UseStaticFiles();
            app.UseHttpsRedirection();

            app.Run();
        }
    }
}
