using System.Linq;
using System.Reactive;
using System.Threading.Tasks;

using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using TransparentCloudServerProxy.Client.Services.Api;
using TransparentCloudServerProxy.Client.Services.Interfaces;
using TransparentCloudServerProxy.Client.ViewModels.Dialogs;
using TransparentCloudServerProxy.Client.ViewModels.Pages;
using TransparentCloudServerProxy.Client.Views.Dialogs;
using TransparentCloudServerProxy.WebDashboard.SqlDb.Models;

namespace TransparentCloudServerProxy.Client.ViewModels.Windows {
    public partial class DashboardWindowViewModel : ViewModel {

        private readonly RemoteServersViewModel _userControlPanelViewModel;
        private readonly IPageRouter _pageRouter;
        private readonly IProxyServerService _proxyServerService;
        private readonly ILoginStorageService _loginStorageService;

        [ObservableProperty]
        public partial ViewModel CurrentPage { get; set; }

        [ObservableProperty]
        public partial ProxyUser CurrentUser { get; set; }

        public ViewModel LocalProxyViewModel { get; }
        public ViewModel AppSettingsViewModel { get; }
        public ViewModel AdminPanelViewModel { get; }

        [ObservableProperty]
        public partial bool CloudServersVisible { get; set; }

        public DashboardWindowViewModel(RemoteServersViewModel userControlPanelViewModel, IPageRouter pageRouter,
            AppSettingsViewModel appSettingsViewModel, AdminPanelViewModel adminPanelViewModel, LocalProxyViewModel localProxyViewModel,
            IProxyServerService proxyServerService, ILoginStorageService loginStorageService) {

            AppSettingsViewModel = appSettingsViewModel;
            AdminPanelViewModel = adminPanelViewModel;
            LocalProxyViewModel = localProxyViewModel;
            _proxyServerService = proxyServerService;
            _loginStorageService = loginStorageService;
            _userControlPanelViewModel = userControlPanelViewModel;
            _pageRouter = pageRouter;

            _pageRouter.OnNavigatePage += (newPage) => CurrentPage = newPage;
            CurrentPage = _userControlPanelViewModel;

            CloudServersVisible = _proxyServerService.GetServerObservableCollection().Count > 0;

            _proxyServerService.GetServerObservableCollection().CollectionChanged += (s, e) => {
                CloudServersVisible = _proxyServerService.GetServerObservableCollection().Count > 0;
            };

            var firstServer = _proxyServerService.GetAllServers().FirstOrDefault(i => i.ServerUser is not null);
            var loggedInUser = firstServer?.ServerUser;

            if (loggedInUser is null) {
                return;
            }

            CurrentUser = loggedInUser;
        }

        [RelayCommand]
        private async Task ShowAddServer() {
            var mainWindow = (Application.Current.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?.MainWindow;
            if (mainWindow == null) {
                return;
            }

            var dialog = new AddServerDialogView();
            var vm = new AddServerDialogViewModel(dialog, _proxyServerService, _loginStorageService);
            dialog.DataContext = vm;

            await dialog.ShowDialog(mainWindow);
        }
    }
}
