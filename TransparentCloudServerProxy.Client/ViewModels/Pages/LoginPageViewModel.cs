using System;
using System.Reactive;
using System.Threading;
using System.Threading.Tasks;

using ReactiveUI;
using ReactiveUI.Fody.Helpers;

using TransparentCloudServerProxy.Client.Extentions;
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

        [Reactive]
        public string Credential { get; set; }

        [Reactive]
        public string ServerAddress { get; set; }

        [Reactive]
        public string ErrorMessage { get; set; }

        public ReactiveCommand<Unit, Unit> LoginCommand { get; }
        public ReactiveCommand<Unit, Unit> LocalCommand { get; }

        public LoginPageViewModel(StartupWindowViewModel startupWindowViewModel, IAuthenticationService authenticationService,
            ILoginStorageService loginStorageService, IPageRouter pageRouter, IdleSpinnerViewModel idleSpinnerViewModel) {
            _startupWindowViewModel = startupWindowViewModel;
            _authenticationService = authenticationService;
            _loginStorageService = loginStorageService;
            _pageRouter = pageRouter;
            _idleSpinnerViewModel = idleSpinnerViewModel;
            LoginCommand = ReactiveCommand.CreateFromTask(LoginAsync);
            LocalCommand = ReactiveCommand.CreateFromTask(LocalSession);
        }

        public void CloseWindow() {
            _startupWindowViewModel.CloseWindow();
        }

        public async Task LocalSession() {

        }

        private async Task LoginAsync() {
            _pageRouter.Navigate(_idleSpinnerViewModel);

            if (_authenticationService.ValidCredentials) {
                CloseWindow();
            }

            var loginSpeedLimit = _rng.Next(500, 2500);
            await Task.Delay(loginSpeedLimit);

            var validLogin = await _authenticationService.LoginAsync(ServerAddress, Credential);
            if (!validLogin) {
                _pageRouter.Navigate(this);
                ErrorMessage = "Invalid login!";
                return;
            }

            _loginStorageService.StoreLogin(Credential, ServerAddress.NormalizeHostUri());
            CloseWindow();
        }
    }
}
