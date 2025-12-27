using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using DynamicData;

using TransparentCloudServerProxy.Client.Models;
using TransparentCloudServerProxy.Client.Services.Api;
using TransparentCloudServerProxy.Client.Services.Interfaces;
using TransparentCloudServerProxy.Models;
using TransparentCloudServerProxy.ProxyBackend;

namespace TransparentCloudServerProxy.Client.ViewModels.Controls {
    public partial class ProxyDataGridViewModel : ViewModel {
        private readonly AppSettingsModel _appSettingsModel;

        public Proxy[] _previousProxies;

        public ObservableCollection<Proxy> DataGridProxies { get; } = new();

        [ObservableProperty]
        public partial Proxy SelectedProxy { get; set; }

        [ObservableProperty]
        public partial IProxyServer ProxyServer { get; set; }

        public string TabName {
            get {
                return ProxyServer.Address.Remove(ProxyServer.Address.Length - 1);
            }
        }

        public ProxyDataGridViewModel(IProxyServer proxyServer, AppSettingsModel appSettingsModel) {
            ProxyServer = proxyServer;
            _appSettingsModel = appSettingsModel;

            _ = SyncGridData();
        }

        public async Task SyncGridData() {
            var currentUser = await ProxyServer.GetUser();
            _previousProxies = DataGridProxies.ToArray();
            DataGridProxies.Clear();

            if (ProxyServer is LocalProxyServer localServer) {
                var localProxies = localServer.GetProxies();

                DataGridProxies.AddRange(localProxies.Cast<Proxy>().Select(i => i.Clone() as Proxy).ToArray());
                return;
            }

            if (currentUser is null) {
                return;
            }

            var savedProxies = currentUser.UserSavedProxies;
            DataGridProxies.AddRange(savedProxies.Select(i => i.GetProxy()));
        }

        [RelayCommand]
        private Task AddAsync() {
            if (ProxyServer is LocalProxyServer) {
                DataGridProxies.Add(new Proxy(_appSettingsModel.LocalDefaultEngine, ProxySocketType.Tcp, "0.0.0.0", 443, "10.0.0.1", 443));
            }
            else {
                DataGridProxies.Add(new Proxy(_appSettingsModel.DefaultCloudEngine, ProxySocketType.Tcp, "0.0.0.0", 443, "10.0.0.1", 443));
            }

            return Task.CompletedTask;
        }

        [RelayCommand]
        private async Task ApplyChangesAsync() {
            var originalProxies = new Dictionary<string, Proxy>();
            foreach (var proxy in _previousProxies) {
                originalProxies.Add($"{proxy.ListenHost}:{proxy.ListenPort}", proxy);
            }

            foreach (var updatedProxy in DataGridProxies) {
                var effectiveKey = $"{updatedProxy.ListenHost}:{updatedProxy.ListenPort}";
                originalProxies.Remove(effectiveKey);

                await ProxyServer.UpdateOrAddProxy(updatedProxy);
            }

            foreach (var deletedProxy in originalProxies.Values) {
                await ProxyServer.DeleteProxy(deletedProxy);
            }

            await SyncGridData();
        }

        [RelayCommand]
        private async Task DeleteAsync() {
            await ProxyServer.DeleteProxy(SelectedProxy);

            await SyncGridData();
        }

        [RelayCommand]
        private async Task ResetChangesAsync() {
            await SyncGridData();
        }
    }
}
