using System;

using TransparentCloudServerProxy.Client.ViewModels;

namespace TransparentCloudServerProxy.Client.Services.Interfaces {
    public interface IPageRouter {
        event Action<ViewModel>? OnNavigatePage;

        void Navigate(ViewModel viewModel);
    }
}