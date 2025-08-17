using ReactiveUI.Fody.Helpers;

using TransparentCloudServerProxy.Client.Services.Interfaces;
using TransparentCloudServerProxy.Client.ViewModels.Pages;
using TransparentCloudServerProxy.WebDashboard.SqlDb.Models;

namespace TransparentCloudServerProxy.Client.ViewModels.Windows {
    public class DashboardWindowViewModel : ViewModel {

        private readonly UserControlPanelViewModel _userControlPanelViewModel;
        private readonly IPageRouter _pageRouter;
        private readonly IAuthenticationService _authenticationService;

        [Reactive]
        public ViewModel CurrentPage { get; set; }

        [Reactive]
        public ProxyUser CurrentUser { get; set; }

        public ViewModel AppSettingsViewModel { get; }

        public DashboardWindowViewModel(UserControlPanelViewModel userControlPanelViewModel, IPageRouter pageRouter,
            IAuthenticationService authenticationService, AppSettingsViewModel appSettingsViewModel) {

            AppSettingsViewModel = appSettingsViewModel;
            _userControlPanelViewModel = userControlPanelViewModel;
            _pageRouter = pageRouter;
            _authenticationService = authenticationService;
            _pageRouter.OnNavigatePage += (newPage) => CurrentPage = newPage;
            CurrentPage = _userControlPanelViewModel;

            var loggedInUser = _authenticationService.GetCurrentUser();
            if (loggedInUser is null) {
                return;
            }

            CurrentUser = loggedInUser;
        }
    }
}
