using System;
using System.Linq;
using System.Reactive;
using System.Threading.Tasks;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using TransparentCloudServerProxy.Client.Services.Interfaces;
using TransparentCloudServerProxy.Client.ViewModels.Windows;

namespace TransparentCloudServerProxy.Client.ViewModels.Pages {
    public partial class LoginPageViewModel : ViewModel {
        private readonly Random _rng = new();

        private readonly StartupWindowViewModel _startupWindowViewModel;
        private readonly IAuthenticationService _authenticationService;
        private readonly ILoginStorageService _loginStorageService;
        private readonly IPageRouter _pageRouter;
        private readonly IdleSpinnerViewModel _idleSpinnerViewModel;
        private readonly IProxyServerService _proxyServerService;

        [ObservableProperty]
        public partial string OneKey { get; set; }

        [ObservableProperty]
        public partial string ErrorMessage { get; set; }

        public LoginPageViewModel(StartupWindowViewModel startupWindowViewModel, IAuthenticationService authenticationService,
            ILoginStorageService loginStorageService, IPageRouter pageRouter, IdleSpinnerViewModel idleSpinnerViewModel,
            IProxyServerService proxyServerService) {
            _startupWindowViewModel = startupWindowViewModel;
            _authenticationService = authenticationService;
            _loginStorageService = loginStorageService;
            _pageRouter = pageRouter;
            _idleSpinnerViewModel = idleSpinnerViewModel;
            _proxyServerService = proxyServerService;
        }

        public void CloseWindow() {
            _startupWindowViewModel.CloseWindow();
        }

        [RelayCommand]
        private Task LocalSession() {
            _pageRouter.Navigate(_idleSpinnerViewModel);
            CloseWindow();

            return Task.CompletedTask;
        }

        [RelayCommand]
        private async Task LoginAsync() {
            if (string.IsNullOrWhiteSpace(OneKey)) {
                ErrorMessage = "Empty OneKey";
                return;
            }

            _pageRouter.Navigate(_idleSpinnerViewModel);

            var loadTask = Task.Run(_authenticationService.LoadAllSavedCredentials);
            loadTask.Wait();

            var allSetupServers = _proxyServerService.GetAllServers();

            if (allSetupServers.Any(i => i.ServerUser != null)) {
                CloseWindow();
                return;
            }

            var loginSpeedLimit = _rng.Next(500, 2500);
            await Task.Delay(loginSpeedLimit);

            var loginServer = await _proxyServerService.AddServer(OneKey);

            if (loginServer is null) {
                _pageRouter.Navigate(this);
                ErrorMessage = "Invalid login!";
                return;
            }


            _loginStorageService.StoreLogin(loginServer.SavedCredential);
            CloseWindow();
        }
    }
}
