using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using TransparentCloudServerProxy.Managed.Models;
using TransparentCloudServerProxy.WebDashboard.Models;
using TransparentCloudServerProxy.WebDashboard.Services;
using TransparentCloudServerProxy.WebDashboard.Services.Interfaces;
using TransparentCloudServerProxy.WebDashboard.SqlDb;

using ProxyConfig = TransparentCloudServerProxy.WebDashboard.Models.ProxyConfig;

namespace TransparentCloudServerProxy.WebDashboard
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Logging.ClearProviders();
            builder.Logging.AddConsole();

            builder.Services.AddControllers();
            builder.Services.AddOpenApi();

            builder.Services
                .AddDbContextFactory<WebDashboardDbContext>(options =>
                    options.UseSqlite("Data Source=proxy.db"));

            builder.Services
                .AddHttpClient();

            builder.Services
                .AddSingleton<IConfigurationRoot>(builder.Configuration)
                .AddSingleton<IProxyConfig, ProxyConfig>()
                .AddSingleton<IProxyService, ProxyService>()
                .AddSingleton<IPublicAddressService, PublicAddressService>()
                .AddSingleton<INetworkInterfaceService, NetworkInterfaceService>()
                .AddSingleton<IUserService, UserService>()
                .AddSingleton<DashboardConfig>()
                .AddSingleton<CredentialsService>();

            builder.Services.AddSingleton<CurrentKestralServerConfig>();
            builder.Services.AddHostedService(sp => sp.GetRequiredService<CurrentKestralServerConfig>());


            builder.Services
                .AddHostedService<DefaultUserService>();

            builder.Services
                .AddAuthorization()
                .AddAuthentication("UserKeyToken")
                .AddScheme<AuthenticationSchemeOptions, UserCredentialAuthenticationHandler>("ClusterToken", options => { })
                .AddScheme<AuthenticationSchemeOptions, UserCredentialAuthenticationHandler>("UserKeyToken", options => { });

            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
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
