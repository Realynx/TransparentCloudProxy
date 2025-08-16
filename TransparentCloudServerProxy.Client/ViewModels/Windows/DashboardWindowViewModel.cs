using ReactiveUI;

using TransparentCloudServerProxy.Client.Services.Interfaces;
using TransparentCloudServerProxy.Client.ViewModels.Pages;

namespace TransparentCloudServerProxy.Client.ViewModels.Windows {
    public class DashboardWindowViewModel : ViewModel {
        private ViewModel _currentPage;
        private readonly UserControlPanelViewModel _userControlPanelViewModel;
        private readonly IPageRouter _pageRouter;

        public ViewModel CurrentPage {
            get {
                return _currentPage;
            }

            set {
                this.RaiseAndSetIfChanged(ref _currentPage, value);
            }
        }

        public DashboardWindowViewModel(UserControlPanelViewModel userControlPanelViewModel, IPageRouter pageRouter) {
            _userControlPanelViewModel = userControlPanelViewModel;
            _pageRouter = pageRouter;

            _pageRouter.OnNavigatePage += (newPage) => CurrentPage = newPage;
            CurrentPage = _userControlPanelViewModel;
        }
    }
}
