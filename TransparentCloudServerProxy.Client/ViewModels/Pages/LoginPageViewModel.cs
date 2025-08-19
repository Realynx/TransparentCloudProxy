using System;
using System.Linq;
using System.Reactive;
using System.Text;
using System.Text.RegularExpressions;
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
        public string OneKey { get; set; }

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

            /*
             * Creds are always 32 bytes
                   Root Cred: 0BF5FFE161F87743EA63B6E4884BC2C26D618EFBC2C1A6C3A8E265CF89B2421D
                 OneKey Pass: 0BF5FFE161F87743EA63B6E4884BC2C26D618EFBC2C1A6C3A8E265CF89B2421D6C6F63616C686F73743A37303032
             */



            // Take 64 nibbles, 32 bytes
            var credential = new string(OneKey.Take(64).ToArray());
            var addressHex = new string(OneKey.Skip(64).ToArray());

            var serverAddressRegex = new Regex(@"https?:\/\/.+?(?=https?:\/\/|$)");
            var addressesString = Encoding.UTF8.GetString(Convert.FromHexString(addressHex));
            var addresses = serverAddressRegex.Matches(addressesString).SelectMany(i => i.Captures);

            var validLogin = false;
            var reachableHost = string.Empty;
            foreach (Capture address in addresses) {
                if (address.Index == 0) {
                    continue;
                }

                validLogin = await _authenticationService.LoginAsync(address.Value, credential);
                if (validLogin) {
                    reachableHost = address.Value;
                    break;
                }
            }

            if (!validLogin) {
                _pageRouter.Navigate(this);
                ErrorMessage = "Invalid login!";
                return;
            }

            _loginStorageService.StoreLogin(credential, reachableHost.NormalizeHostUri());
            CloseWindow();
        }
    }
}
