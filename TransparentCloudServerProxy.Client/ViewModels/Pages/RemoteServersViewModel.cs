using System.Collections.ObjectModel;
using System.Reactive;
using System.Threading.Tasks;

using DynamicData;

using Microsoft.AspNetCore.Hosting.Server;

using ReactiveUI;
using ReactiveUI.Fody.Helpers;

using TransparentCloudServerProxy.Client.Services.Api;
using TransparentCloudServerProxy.Client.Services.Interfaces;
using TransparentCloudServerProxy.Client.ViewModels.Controls;

namespace TransparentCloudServerProxy.Client.ViewModels.Pages {
    public class RemoteServersViewModel : ViewModel {
        private IProxyServerService _proxyServerService { get; set; }

        public ObservableCollection<ProxyServer> Servers { get; } = new();

        public ObservableCollection<ProxyDataGridViewModel> ServerDataGrids { get; } = new();



        [Reactive]
        public bool ApplyChangesVisible { get; set; }

        public ReactiveCommand<Unit, Unit> DeleteCommand { get; }
        public ReactiveCommand<Unit, Unit> ResetChanges { get; }


        public RemoteServersViewModel(IProxyServerService proxyServerService) {
            _proxyServerService = proxyServerService;

            DeleteCommand = ReactiveCommand.CreateFromTask(DeleteCommandAsync);
            ResetChanges = ReactiveCommand.CreateFromTask(ResetChangesAsync);

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

        public async Task DeleteCommandAsync() {

        }

        public async Task ResetChangesAsync() {
        }
    }
}

