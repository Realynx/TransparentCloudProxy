using System;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;

using ReactiveUI;

using TransparentCloudServerProxy.Client.Services.Interfaces;
using TransparentCloudServerProxy.Client.ViewModels.Pages;

namespace TransparentCloudServerProxy.Client.ViewModels.Windows {
    public class StartupWindowViewModel : ViewModel {
        private readonly IServiceProvider _serviceProvider;
        public event Action? StartupCompleted;

        private ViewModel _currentPage;
        public ViewModel CurrentPage {
            get {
                return _currentPage;
            }

            set {
                this.RaiseAndSetIfChanged(ref _currentPage, value);
            }
        }

        public StartupWindowViewModel(IServiceProvider serviceProvider) {

            CurrentPage = new IdleSpinnerViewModel("Checking credentials...");
            _serviceProvider = serviceProvider;
        }

        public void CloseWindow() {
            StartupCompleted?.Invoke();
        }

        public async Task InitializeAsync() {
            var authCredential = _serviceProvider.GetRequiredService<IAuthenticationService>();
            var validAuth = await authCredential.CheckCredential();

            if (validAuth) {
                var mainWindow = new DashboardWindow {
                    DataContext = _serviceProvider.GetRequiredService<DashboardWindowViewModel>()
                };
                mainWindow.Show();
            }
            else {
                CurrentPage = new LoginPageViewModel(this, authCredential);
            }
        }

    }

}
