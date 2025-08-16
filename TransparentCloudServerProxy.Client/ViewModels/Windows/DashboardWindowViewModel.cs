using ReactiveUI;

using TransparentCloudServerProxy.Client.ViewModels.Pages;

namespace TransparentCloudServerProxy.Client.ViewModels.Windows {
    public class DashboardWindowViewModel : ViewModel {
        private ViewModel _currentPage;
        public ViewModel CurrentPage {
            get {
                return _currentPage;
            }

            set {
                this.RaiseAndSetIfChanged(ref _currentPage, value);
            }
        }

        public DashboardWindowViewModel() {
            _currentPage = new IdleSpinnerViewModel();
            CurrentPage = _currentPage;
        }
    }
}
