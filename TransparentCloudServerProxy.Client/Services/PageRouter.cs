using System;

using TransparentCloudServerProxy.Client.Services.Interfaces;
using TransparentCloudServerProxy.Client.ViewModels;

namespace TransparentCloudServerProxy.Client.Services {
    public class PageRouter : IPageRouter {
        public event Action<ViewModel>? OnNavigatePage;

        public PageRouter() {

        }

        public void Navigate(ViewModel viewModel) {
            OnNavigatePage?.Invoke(viewModel);
        }
    }
}
