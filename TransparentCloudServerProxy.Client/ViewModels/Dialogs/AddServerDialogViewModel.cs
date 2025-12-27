using System.Threading.Tasks;

using Avalonia.Controls;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using TransparentCloudServerProxy.Client.Services.Interfaces;

namespace TransparentCloudServerProxy.Client.ViewModels.Dialogs {
    public partial class AddServerDialogViewModel : ViewModel {
        private readonly Window _window;
        private readonly IProxyServerService _proxyServerService;
        private readonly ILoginStorageService _loginStorageService;

        [ObservableProperty]
        public partial string OneKey { get; set; }

        [ObservableProperty]
        public partial string ErrorMessage { get; set; }

        public AddServerDialogViewModel(Window window, IProxyServerService proxyServerService, ILoginStorageService loginStorageService) {
            _window = window;
            _proxyServerService = proxyServerService;
            _loginStorageService = loginStorageService;
        }

        [RelayCommand]
        private async Task LoginAsync() {
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
