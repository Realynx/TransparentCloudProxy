using System.Reactive;
using System.Threading.Tasks;

using ReactiveUI;

using TransparentCloudServerProxy.Client.Services.Interfaces;
using TransparentCloudServerProxy.Client.ViewModels.Windows;

namespace TransparentCloudServerProxy.Client.ViewModels.Pages {
    public class LoginPageViewModel : ViewModel {
        private readonly StartupWindowViewModel _startupWindowViewModel;
        private readonly IAuthenticationService _authenticationService;
        private string _credential;
        public string Credential {
            get => _credential;
            set => this.RaiseAndSetIfChanged(ref _credential, value);
        }

        private string _serverAddress;
        public string ServerAddress {
            get => _serverAddress;
            set => this.RaiseAndSetIfChanged(ref _serverAddress, value);
        }

        private string _errorMessage;
        public string ErrorMessage {
            get => _errorMessage;
            set => this.RaiseAndSetIfChanged(ref _errorMessage, value);
        }

        public ReactiveCommand<Unit, Unit> LoginCommand { get; }

        public LoginPageViewModel(StartupWindowViewModel startupWindowViewModel, IAuthenticationService authenticationService) {
            _startupWindowViewModel = startupWindowViewModel;
            _authenticationService = authenticationService;
            LoginCommand = ReactiveCommand.CreateFromTask(LoginAsync);
        }

        public void CloseWindow() {
            _startupWindowViewModel.CloseWindow();
        }

        private async Task LoginAsync() {
            if (_authenticationService.ValidCredentials) {
                CloseWindow();
            }

            var validLogin = await _authenticationService.LoginAsync(ServerAddress, Credential);
            if (!validLogin) {
                ErrorMessage = "Invalid login!";
                return;
            }

            CloseWindow();
        }
    }
}
