using System;

using Microsoft.Extensions.DependencyInjection;

using ReactiveUI;

using TransparentCloudServerProxy.Client.ViewModels.Pages;

namespace TransparentCloudServerProxy.Client.ViewModels.Windows {
    public class DashboardWindowViewModel : ViewModel {
        private ViewModel _currentPage;
        private readonly IServiceProvider _serviceProvider;

        public ViewModel CurrentPage {
            get {
                return _currentPage;
            }

            set {
                this.RaiseAndSetIfChanged(ref _currentPage, value);
            }
        }

        public DashboardWindowViewModel(IServiceProvider serviceProvider) {
            var userControlViewModel = serviceProvider.GetRequiredService<UserControlPanelViewModel>();

            CurrentPage = userControlViewModel;
            _serviceProvider = serviceProvider;
        }
    }
}
