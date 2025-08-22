using System;
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
using TransparentCloudServerProxy.WebDashboard.SqlDb.Models;

namespace TransparentCloudServerProxy.Client.ViewModels.Controls {
    public class ProxyDataGridViewModel : ViewModel {
        [Reactive]
        public ObservableCollection<Proxy> DataGridProxies { get; set; } = new();
        public Proxy[] _previousProxies;

        [Reactive]
        public Proxy SelectedProxy { get; set; }

        [Reactive]
        public IProxyServer ProxyServer { get; set; }

        public string TabName {
            get {
                return ProxyServer.Address.Remove(ProxyServer.Address.Length - 1);
            }
        }

        public ReactiveCommand<Unit, Unit> AddCommand { get; }
        public ReactiveCommand<Unit, Unit> DeleteCommand { get; }
        public ReactiveCommand<Unit, Unit> ResetChanges { get; }
        public ReactiveCommand<Unit, Unit> ApplyChanges { get; }


        public ProxyDataGridViewModel(IProxyServer proxyServer) {
            ProxyServer = proxyServer;
            ApplyChanges = ReactiveCommand.CreateFromTask(ApplyChangesAsync);
            AddCommand = ReactiveCommand.CreateFromTask(AddCommandAsync);
            DeleteCommand = ReactiveCommand.CreateFromTask(DeleteCommandAsync);
            ResetChanges = ReactiveCommand.CreateFromTask(ResetChangesAsync);

            _ = SyncGridData();
        }

        public async Task SyncGridData() {
            var currentUser = await ProxyServer.GetUser();
            _previousProxies = DataGridProxies.ToArray();
            DataGridProxies.Clear();

            if (ProxyServer is LocalProxyServer localServer) {
                var localProxies = localServer.GetProxies();
                // TODO: This needs to go lol. Use ICloneable
                DataGridProxies.AddRange(localProxies.Select(i => new SavedProxy((Proxy)i, Guid.Empty).GetProxy()));
                return;
            }

            if (currentUser is null) {
                return;
            }

            var savedProxies = currentUser.UserSavedProxies;
            DataGridProxies.AddRange(savedProxies.Select(i => i.GetProxy()));
        }

        public Task AddCommandAsync() {
            DataGridProxies.Add(new Proxy(PacketEngine.NativeC, Managed.Models.ProxySocketType.Tcp, "0.0.0.0", 443, "10.0.0.1", 443));
            return Task.CompletedTask;
        }

        public async Task ApplyChangesAsync() {
            var originalProxies = new Dictionary<string, Proxy>();
            foreach (var proxy in _previousProxies) {
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

            await SyncGridData();
        }

        public async Task DeleteCommandAsync() {
            await ProxyServer.DeleteProxy(SelectedProxy);

            await SyncGridData();
        }

        public async Task ResetChangesAsync() {
            await SyncGridData();
        }
    }
}
