using System;

using Microsoft.Extensions.DependencyInjection;

using TransparentCloudServerProxy.Client.Services.Interfaces;
using TransparentCloudServerProxy.Client.Services;
using TransparentCloudServerProxy.Client.ViewModels.Pages;
using TransparentCloudServerProxy.Client.ViewModels.Windows;
using Polly.Extensions.Http;
using Polly;
using System.Net.Http;
using TransparentCloudServerProxy.Client.Models;
using TransparentCloudServerProxy.Client.Services.Api;
using TransparentCloudServerProxy.Client.ViewModels.Controls;

namespace TransparentCloudServerProxy.Client {
    internal static class Startup {
        public static IServiceCollection ConfigureServices(IServiceCollection services) {
            AddViewModels(services)
                .AddHttpClient(nameof(IProxyServerFactory), client => {
                    client.DefaultRequestHeaders.Add("Accept", "application/json");
                    client.DefaultRequestHeaders.UserAgent.Clear();
                    client.DefaultRequestHeaders.UserAgent.ParseAdd($"RealynxProxy/1.0 ({Environment.OSVersion.Platform}; {Environment.OSVersion.Version})");
                })
                .ConfigurePrimaryHttpMessageHandler(() => {
#if DEBUG
                    return new HttpClientHandler {
                        ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
                    };
#else
                    return new HttpClientHandler();
#endif

                })
                .AddPolicyHandler(GetRetryPolicy());

            services
                .AddSingleton<IAuthenticationService, AuthenticationService>()
                .AddSingleton<IPageRouter, PageRouter>()
                .AddSingleton<IProxyServerFactory, ProxyServerFactory>()
                .AddSingleton<IProxyServerService, ProxyServerService>()
                .AddSingleton<ICryptoService, CryptoService>()
                .AddSingleton<ISecureFileStorageService, SecureFileStorageService>()
                .AddSingleton<ILoginStorageService, LoginStorageService>();

            return services;
        }

        private static IServiceCollection AddViewModels(IServiceCollection services) {
            services
                .AddTransient<StartupWindowViewModel>()
                .AddTransient<DashboardWindowViewModel>()
                .AddTransient<IdleSpinnerViewModel>()
                .AddTransient<LoginPageViewModel>()
                .AddTransient<RemoteServersViewModel>()
                .AddTransient<AppSettingsViewModel>()
                .AddTransient<AdminPanelViewModel>()
                .AddTransient<ProxyDataGridViewModel>()

                .AddSingleton<AppSettingsModel>();

            return services;
        }

        private static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy() {
            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .WaitAndRetryAsync(6, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
        }
    }
}
