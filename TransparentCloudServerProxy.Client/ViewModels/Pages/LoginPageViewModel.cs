using System;
using System.Linq;
using System.Reactive;
using System.Threading.Tasks;

using ReactiveUI;
using ReactiveUI.Fody.Helpers;

using TransparentCloudServerProxy.Client.Services.Interfaces;
using TransparentCloudServerProxy.Client.ViewModels.Windows;

namespace TransparentCloudServerProxy.Client.ViewModels.Pages {
    public class LoginPageViewModel : ViewModel {
        private readonly Random _rng = new();

        private readonly StartupWindowViewModel _startupWindowViewModel;
        private readonly IAuthenticationService _authenticationService;
        private readonly ILoginStorageService _loginStorageService;
        private readonly IPageRouter _pageRouter;
        private readonly IdleSpinnerViewModel _idleSpinnerViewModel;
        private readonly IProxyServerService _proxyServerService;

        [Reactive]
        public string OneKey { get; set; }

        [Reactive]
        public string ErrorMessage { get; set; }

        public ReactiveCommand<Unit, Unit> LoginCommand { get; }
        public ReactiveCommand<Unit, Unit> LocalCommand { get; }

        public LoginPageViewModel(StartupWindowViewModel startupWindowViewModel, IAuthenticationService authenticationService,
            ILoginStorageService loginStorageService, IPageRouter pageRouter, IdleSpinnerViewModel idleSpinnerViewModel,
            IProxyServerService proxyServerService) {
            _startupWindowViewModel = startupWindowViewModel;
            _authenticationService = authenticationService;
            _loginStorageService = loginStorageService;
            _pageRouter = pageRouter;
            _idleSpinnerViewModel = idleSpinnerViewModel;
            _proxyServerService = proxyServerService;

            LoginCommand = ReactiveCommand.CreateFromTask(LoginAsync);
            LocalCommand = ReactiveCommand.CreateFromTask(LocalSession);
        }

        public void CloseWindow() {
            _startupWindowViewModel.CloseWindow();
        }

        public async Task LocalSession() {
            _pageRouter.Navigate(_idleSpinnerViewModel);
            CloseWindow();
        }

        private async Task LoginAsync() {
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
