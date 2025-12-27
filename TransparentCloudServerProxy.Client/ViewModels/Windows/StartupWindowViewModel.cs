using System;
using System.Linq;
using System.Threading.Tasks;

using CommunityToolkit.Mvvm.ComponentModel;

using Microsoft.Extensions.DependencyInjection;

using TransparentCloudServerProxy.Client.Services.Interfaces;
using TransparentCloudServerProxy.Client.ViewModels.Pages;

namespace TransparentCloudServerProxy.Client.ViewModels.Windows {
    public partial class StartupWindowViewModel : ViewModel {
        private readonly IServiceProvider _serviceProvider;
        private readonly IPageRouter _pageRouter;

        public event Action? StartupCompleted;

        [ObservableProperty]
        public partial ViewModel CurrentPage { get; set; }

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

            var loadingSavedCredentials = Task.Run(authCredential.LoadAllSavedCredentials);
            loadingSavedCredentials.Wait();

            var validAuth = proxyServerService.GetAllServers().Any(i => i.ServerUser is not null);
            if (validAuth) {
                CloseWindow();
                return;
            }

            var storageService = _serviceProvider.GetRequiredService<ILoginStorageService>();
            var pageRouter = _serviceProvider.GetRequiredService<IPageRouter>();
            var oneKeyService = _serviceProvider.GetRequiredService<IOneKeyService>();
            var idleSpinnerViewModel = _serviceProvider.GetRequiredService<IdleSpinnerViewModel>();

            CurrentPage = new LoginPageViewModel(this, authCredential, storageService,
                pageRouter, idleSpinnerViewModel, proxyServerService);
        }

    }

}
