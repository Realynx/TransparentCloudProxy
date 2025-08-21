using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Threading.Tasks;

using DynamicData;

using ReactiveUI;
using ReactiveUI.Fody.Helpers;

using TransparentCloudServerProxy.Client.Services.Api;
using TransparentCloudServerProxy.ProxyBackend;

namespace TransparentCloudServerProxy.Client.ViewModels.Controls {
    public class ProxyDataGridViewModel : ViewModel {
        [Reactive]
        public ObservableCollection<Proxy> DataGridProxies { get; set; } = new();

        [Reactive]
        public Proxy SelectedProxy { get; set; }

        [Reactive]
        public ProxyServer ProxyServer { get; set; }

        public string TabName {
            get {
                return ProxyServer.Address.Remove(ProxyServer.Address.Length - 1);
            }
        }

        public ReactiveCommand<Unit, Unit> AddCommand { get; }
        public ReactiveCommand<Unit, Unit> ApplyChanges { get; }

        public ProxyDataGridViewModel(ProxyServer proxyServer) {
            ProxyServer = proxyServer;
            ApplyChanges = ReactiveCommand.CreateFromTask(ApplyChangesAsync);
            AddCommand = ReactiveCommand.CreateFromTask(AddCommandAsync);

            SyncGridData();
        }

        public void SyncGridData() {
            var savedProxies = ProxyServer.ServerUser.UserSavedProxies;

            DataGridProxies.Clear();
            DataGridProxies.AddRange(savedProxies.Select(i => i.GetProxy()));
        }

        public async Task AddCommandAsync() {
            DataGridProxies.Add(new Proxy(PacketEngine.NativeC, Managed.Models.ProxySocketType.Tcp, "0.0.0.0", 443, "10.0.0.1", 443));
        }

        public async Task ApplyChangesAsync() {
            var originalProxies = new Dictionary<string, Proxy>();
            foreach (var proxy in ProxyServer.ServerUser.UserSavedProxies.Select(i => i.GetProxy())) {
                originalProxies.Add($"{proxy.ListenHost}:{proxy.ListenPort}", proxy);
            }

            foreach (var updatedProxy in DataGridProxies) {
                var effectiveKey = $"{updatedProxy.ListenHost}:{updatedProxy.ListenPort}";
                if (originalProxies.ContainsKey(effectiveKey)) {
                    originalProxies.Remove(effectiveKey);
                }

                await ProxyServer.UpdateOrAddProxy(updatedProxy);
            }

            foreach (var deletedProxy in originalProxies.Values) {
                await ProxyServer.DeleteProxy(deletedProxy);
            }

            SyncGridData();
        }
    }
}
