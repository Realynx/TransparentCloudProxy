using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

using CommunityToolkit.Mvvm.ComponentModel;

using DynamicData;

using TransparentCloudServerProxy.Client.Models;
using TransparentCloudServerProxy.Client.Services.Api;
using TransparentCloudServerProxy.Client.Services.Interfaces;
using TransparentCloudServerProxy.Client.ViewModels.Controls;

namespace TransparentCloudServerProxy.Client.ViewModels.Pages {
    public partial class RemoteServersViewModel : ViewModel {
        public ObservableCollection<ProxyServer> Servers { get; set; }

        public ReadOnlyObservableCollection<ProxyDataGridViewModel> ServerDataGrids { get; }

        [ObservableProperty]
        public partial bool ApplyChangesVisible { get; set; }

        private readonly IProxyServerService _proxyServerService;
        private readonly SourceList<ProxyServer> _sourceList;
        private readonly IDisposable _cleanup;

        public RemoteServersViewModel(IProxyServerService proxyServerService, AppSettingsModel appSettingsModel) {
            _proxyServerService = proxyServerService;
            Servers = _proxyServerService.GetServerObservableCollection();

            _sourceList = new SourceList<ProxyServer>();
            _sourceList.AddRange(Servers);

            _cleanup = _sourceList.Connect()
                .Transform(server => new ProxyDataGridViewModel(server, appSettingsModel))
                .Bind(out var serverDataGrids)
                .Subscribe();

            ServerDataGrids = serverDataGrids;

            Servers.CollectionChanged += Servers_CollectionChanged;
        }

        private void Servers_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e) {
            if (e.NewItems != null) {
                foreach (ProxyServer server in e.NewItems)
                    _sourceList.Add(server);
            }

            if (e.OldItems != null) {
                foreach (ProxyServer server in e.OldItems)
                    _sourceList.Remove(server);
            }
        }
    }
}
