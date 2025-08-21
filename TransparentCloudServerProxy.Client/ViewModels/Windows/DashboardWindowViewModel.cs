using System.Linq;

using ReactiveUI.Fody.Helpers;

using TransparentCloudServerProxy.Client.Services.Interfaces;
using TransparentCloudServerProxy.Client.ViewModels.Pages;
using TransparentCloudServerProxy.WebDashboard.SqlDb.Models;

namespace TransparentCloudServerProxy.Client.ViewModels.Windows {
    public class DashboardWindowViewModel : ViewModel {

        private readonly RemoteServersViewModel _userControlPanelViewModel;
        private readonly IPageRouter _pageRouter;
        private readonly IProxyServerService _proxyServerService;

        [Reactive]
        public ViewModel CurrentPage { get; set; }

        [Reactive]
        public ProxyUser CurrentUser { get; set; }

        public ViewModel AppSettingsViewModel { get; }
        public ViewModel AdminPanelViewModel { get; }

        public DashboardWindowViewModel(RemoteServersViewModel userControlPanelViewModel, IPageRouter pageRouter,
            AppSettingsViewModel appSettingsViewModel, AdminPanelViewModel adminPanelViewModel, IProxyServerService proxyServerService) {

            AppSettingsViewModel = appSettingsViewModel;
            AdminPanelViewModel = adminPanelViewModel;
            _proxyServerService = proxyServerService;
            _userControlPanelViewModel = userControlPanelViewModel;
            _pageRouter = pageRouter;
            _pageRouter.OnNavigatePage += (newPage) => CurrentPage = newPage;
            CurrentPage = _userControlPanelViewModel;

            var firstServer = _proxyServerService.GetAllServers().FirstOrDefault(i => i.ServerUser is not null);
            var loggedInUser = firstServer?.ServerUser;

            if (loggedInUser is null) {
                return;
            }

            CurrentUser = loggedInUser;
        }
    }
}
