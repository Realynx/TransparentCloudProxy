using System;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;

using ReactiveUI;

using TransparentCloudServerProxy.Client.Services.Interfaces;
using TransparentCloudServerProxy.Client.ViewModels.Pages;

namespace TransparentCloudServerProxy.Client.ViewModels.Windows {
    public class StartupWindowViewModel : ViewModel {
        private readonly IServiceProvider _serviceProvider;
        private readonly IPageRouter _pageRouter;

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

        public StartupWindowViewModel(IServiceProvider serviceProvider, IPageRouter pageRouter) {
            _pageRouter = pageRouter;
            _pageRouter.OnNavigatePage += (newPage) => CurrentPage = newPage;

            CurrentPage = new IdleSpinnerViewModel("Checking credentials...");
            _serviceProvider = serviceProvider;
        }

        public void CloseWindow() {
            StartupCompleted?.Invoke();
        }

        public void Initialize() {
            var proxyServerService = _serviceProvider.GetRequiredService<IProxyServerService>();
            var authCredential = _serviceProvider.GetRequiredService<IAuthenticationService>();

            var validAuth = proxyServerService.GetAllServers().Any(i => i.ServerUser is not null);
            if (validAuth) {
                CloseWindow();
                return;
            }

            var storageService = _serviceProvider.GetRequiredService<ILoginStorageService>();
            var pageRouter = _serviceProvider.GetRequiredService<IPageRouter>();
            var idleSpinnerViewModel = _serviceProvider.GetRequiredService<IdleSpinnerViewModel>();
            CurrentPage = new LoginPageViewModel(this, authCredential, storageService, pageRouter, idleSpinnerViewModel, proxyServerService);
        }

    }

}
