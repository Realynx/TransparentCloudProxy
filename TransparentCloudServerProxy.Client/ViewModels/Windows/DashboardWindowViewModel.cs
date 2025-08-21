using System.Linq;
using System.Reactive;
using System.Threading.Tasks;

using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;

using ReactiveUI;
using ReactiveUI.Fody.Helpers;

using TransparentCloudServerProxy.Client.Services.Interfaces;
using TransparentCloudServerProxy.Client.ViewModels.Dialogs;
using TransparentCloudServerProxy.Client.ViewModels.Pages;
using TransparentCloudServerProxy.Client.Views.Dialogs;
using TransparentCloudServerProxy.WebDashboard.SqlDb.Models;

namespace TransparentCloudServerProxy.Client.ViewModels.Windows {
    public class DashboardWindowViewModel : ViewModel {

        private readonly RemoteServersViewModel _userControlPanelViewModel;
        private readonly IPageRouter _pageRouter;
        private readonly IProxyServerService _proxyServerService;
        private readonly ILoginStorageService _loginStorageService;


        [Reactive]
        public ViewModel CurrentPage { get; set; }

        [Reactive]
        public ProxyUser CurrentUser { get; set; }

        public ViewModel AppSettingsViewModel { get; }
        public ViewModel AdminPanelViewModel { get; }

        public ReactiveCommand<Unit, Unit> AddServer { get; }


        public DashboardWindowViewModel(RemoteServersViewModel userControlPanelViewModel, IPageRouter pageRouter,
            AppSettingsViewModel appSettingsViewModel, AdminPanelViewModel adminPanelViewModel,
            IProxyServerService proxyServerService, ILoginStorageService loginStorageService) {

            AppSettingsViewModel = appSettingsViewModel;
            AdminPanelViewModel = adminPanelViewModel;
            _proxyServerService = proxyServerService;
            _loginStorageService = loginStorageService;
            _userControlPanelViewModel = userControlPanelViewModel;
            _pageRouter = pageRouter;

            AddServer = ReactiveCommand.CreateFromTask(ShowAddServerDialog);

            _pageRouter.OnNavigatePage += (newPage) => CurrentPage = newPage;
            CurrentPage = _userControlPanelViewModel;

            var firstServer = _proxyServerService.GetAllServers().FirstOrDefault(i => i.ServerUser is not null);
            var loggedInUser = firstServer?.ServerUser;

            if (loggedInUser is null) {
                return;
            }

            CurrentUser = loggedInUser;
        }

        public async Task ShowAddServerDialog() {
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
