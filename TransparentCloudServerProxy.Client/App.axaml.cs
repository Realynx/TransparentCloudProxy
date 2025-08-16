using System;
using System.Net.Http;

using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;

using Microsoft.Extensions.DependencyInjection;

using Polly;
using Polly.Extensions.Http;

using TransparentCloudServerProxy.Client.Services;
using TransparentCloudServerProxy.Client.Services.Interfaces;
using TransparentCloudServerProxy.Client.ViewModels.Pages;
using TransparentCloudServerProxy.Client.ViewModels.Windows;
using TransparentCloudServerProxy.Client.Views.Windows;

namespace TransparentCloudServerProxy.Client {
    public partial class App : Application {
        public override void Initialize() {
            AvaloniaXamlLoader.Load(this);

            DataTemplates.Add(new ViewLocator());
        }

        public override void OnFrameworkInitializationCompleted() {
            var services = new ServiceCollection();
            ConfigureServices(services);
            AddViewModels(services);

            var serviceProvider = services.BuildServiceProvider();

            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop) {
                var startupWindow = new StartupWindowView {
                    DataContext = serviceProvider.GetRequiredService<StartupWindowViewModel>()
                };

                desktop.MainWindow = startupWindow;

                var vm = (StartupWindowViewModel)startupWindow.DataContext;
                _ = vm.InitializeAsync();

                vm.StartupCompleted += () => {
                    var mainWindow = new DashboardWindow {
                        DataContext = serviceProvider.GetRequiredService<DashboardWindowViewModel>()
                    };

                    mainWindow.Show();
                    startupWindow.Close();
                };
            }

            base.OnFrameworkInitializationCompleted();
        }

        private static void AddViewModels(ServiceCollection services) {
            services
                .AddTransient<StartupWindowViewModel>()
                .AddTransient<DashboardWindowViewModel>()
                .AddTransient<IdleSpinnerViewModel>()
                .AddTransient<LoginPageViewModel>();
        }

        private static void ConfigureServices(ServiceCollection services) {
            services
                .AddHttpClient<UserApi>(client => {
                    client.DefaultRequestHeaders.Add("Accept", "application/json");
                    client.DefaultRequestHeaders.UserAgent.Clear();
                    client.DefaultRequestHeaders.UserAgent.ParseAdd($"RealynxProxy/1.0 {Environment.OSVersion.Platform}; {Environment.OSVersion.Version}");
                }).AddPolicyHandler(GetRetryPolicy());

            services
                .AddSingleton<IAuthenticationService, AuthenticationService>()
                .AddSingleton<IUserApi, UserApi>();
        }

        static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy() {
            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .WaitAndRetryAsync(6, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
        }
    }
}