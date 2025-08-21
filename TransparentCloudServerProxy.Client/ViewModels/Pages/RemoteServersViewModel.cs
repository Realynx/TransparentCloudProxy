using System.Collections.ObjectModel;

using DynamicData;

using ReactiveUI.Fody.Helpers;

using TransparentCloudServerProxy.Client.Services.Api;
using TransparentCloudServerProxy.Client.Services.Interfaces;
using TransparentCloudServerProxy.Client.ViewModels.Controls;

namespace TransparentCloudServerProxy.Client.ViewModels.Pages {
    public class RemoteServersViewModel : ViewModel {
        public ObservableCollection<ProxyServer> Servers { get; } = new();

        public ObservableCollection<ProxyDataGridViewModel> ServerDataGrids { get; } = new();

        [Reactive]
        public bool ApplyChangesVisible { get; set; }


        private readonly IProxyServerService _proxyServerService;
        public RemoteServersViewModel(IProxyServerService proxyServerService) {
            _proxyServerService = proxyServerService;

            SyncServers();
        }

        private void SyncServers() {
            var servers = _proxyServerService.GetAllServers();

            Servers.Clear();
            Servers.AddRange(servers);

            foreach (var proxyServer in Servers) {
                ServerDataGrids.Add(new ProxyDataGridViewModel(proxyServer));
            }
        }
    }
}

