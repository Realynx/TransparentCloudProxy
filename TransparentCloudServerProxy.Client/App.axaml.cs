using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;

using Microsoft.Extensions.DependencyInjection;

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

            var serviceProvider = Startup.ConfigureServices(services)
                .BuildServiceProvider();

            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop) {
                var startupWindow = new StartupWindowView {
                    DataContext = serviceProvider.GetRequiredService<StartupWindowViewModel>()
                };

                desktop.MainWindow = startupWindow;
                var startupWindowViewModel = (StartupWindowViewModel)startupWindow.DataContext;
                startupWindowViewModel.StartupCompleted += () => {
                    var mainWindow = new DashboardWindow {
                        DataContext = serviceProvider.GetRequiredService<DashboardWindowViewModel>()
                    };

                    Dispatcher.UIThread.Invoke(() => {
                        desktop.MainWindow = mainWindow;
                        mainWindow.Show();
                        startupWindow.Close();
                    });
                };

                _ = startupWindowViewModel.InitializeAsync();
            }
            base.OnFrameworkInitializationCompleted();
        }
    }
}