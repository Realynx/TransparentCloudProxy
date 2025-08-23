using TransparentCloudServerProxy.Client.Models;
using TransparentCloudServerProxy.Client.Services.Api;
using TransparentCloudServerProxy.Client.ViewModels.Controls;
using TransparentCloudServerProxy.Interfaces;
using TransparentCloudServerProxy.Services;

namespace TransparentCloudServerProxy.Client.ViewModels.Pages {
    public class LocalProxyViewModel : ViewModel {
        private readonly IProxyService _proxyService;

        public ProxyDataGridViewModel LocalProxyDataGridViewModel { get; set; }
        public LocalProxyViewModel(IProxyService proxyService, AppSettingsModel appSettingsModel) {
            _proxyService = proxyService;

            LocalProxyDataGridViewModel = new ProxyDataGridViewModel(new LocalProxyServer(_proxyService), appSettingsModel);
        }
    }
}
