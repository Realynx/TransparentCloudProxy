using System;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;

using ReactiveUI;

using TransparentCloudServerProxy.Client.Services.Interfaces;
using TransparentCloudServerProxy.Client.ViewModels.Pages;

namespace TransparentCloudServerProxy.Client.ViewModels.Windows {
    public class StartupWindowViewModel : ViewModel {
        private readonly IAuthenticationService _authService;
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

        public StartupWindowViewModel(IAuthenticationService authService, IServiceProvider sp) {
            _authService = authService;
            _serviceProvider = sp;

            CurrentPage = new IdleSpinnerViewModel("Checking credentials...");
        }

        public void CloseWindow() {
            StartupCompleted?.Invoke();
        }

        public async Task InitializeAsync() {
            var validAuth = await _authService.CheckCredential();

            if (validAuth) {
                var mainWindow = new DashboardWindow {
                    DataContext = _serviceProvider.GetRequiredService<DashboardWindowViewModel>()
                };
                mainWindow.Show();
            }
            else {
                CurrentPage = new LoginPageViewModel(this, _authService);
            }
        }

    }

}
