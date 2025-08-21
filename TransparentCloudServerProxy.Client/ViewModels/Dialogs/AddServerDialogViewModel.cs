using System.Reactive;
using System.Threading.Tasks;

using Avalonia.Controls;

using ReactiveUI;
using ReactiveUI.Fody.Helpers;

using TransparentCloudServerProxy.Client.Services.Interfaces;

namespace TransparentCloudServerProxy.Client.ViewModels.Dialogs {
    public class AddServerDialogViewModel : ReactiveObject {
        private readonly Window _window;
        private readonly IProxyServerService _proxyServerService;
        private readonly ILoginStorageService _loginStorageService;

        [Reactive]
        public string OneKey { get; set; }

        [Reactive]
        public string ErrorMessage { get; set; }

        public ReactiveCommand<Unit, Unit> LoginCommand { get; }
        public ReactiveCommand<Unit, Unit> CancelCommand { get; }

        public AddServerDialogViewModel(Window window, IProxyServerService proxyServerService, ILoginStorageService loginStorageService) {
            _window = window;
            _proxyServerService = proxyServerService;
            _loginStorageService = loginStorageService;
            LoginCommand = ReactiveCommand.CreateFromTask(LoginAsync);
        }

        public async Task LoginAsync() {
            var loginServer = await _proxyServerService.AddServer(OneKey);

            if (loginServer is null) {
                ErrorMessage = "Invalid login!";
                return;
            }

            _loginStorageService.StoreLogin(loginServer.SavedCredential);
            _window.Close();
        }
    }
}
