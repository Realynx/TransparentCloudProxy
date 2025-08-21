using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using TransparentCloudServerProxy.Client.Extentions;
using TransparentCloudServerProxy.Client.Models;
using TransparentCloudServerProxy.Client.Services.Interfaces;

namespace TransparentCloudServerProxy.Client.Services.Api {
    public class ProxyServerService : IProxyServerService {

        private readonly ObservableCollection<ProxyServer> _proxyServers = new();
        private readonly IProxyServerFactory _proxyServerFactory;
        private readonly IOneKeyService _oneKeyService;

        public ProxyServerService(IProxyServerFactory proxyServerFactory, IOneKeyService oneKeyService) {
            _proxyServerFactory = proxyServerFactory;
            _oneKeyService = oneKeyService;
        }

        public async Task<ProxyServer?> AddServer(string oneKey) {
            (var addresses, var credential) = _oneKeyService.DecodeOneKey(oneKey);
            foreach (var address in addresses) {
                var serverLogin = await AddServer(new SavedCredential() {
                    ReachableAddress = address.NormalizeHostUri().ToString(),
                    Credential = credential
                });

                if (serverLogin is not null) {
                    return serverLogin;
                }
            }

            return null;
        }

        public async Task<ProxyServer?> AddServer(SavedCredential savedCredential) {
            var proxyServer = _proxyServerFactory.CreateProxyServer(savedCredential);

            if (proxyServer is null) {
                return null;
            }

            try {
                var authenticatedUser = await proxyServer.GetUser(new CancellationTokenSource(TimeSpan.FromSeconds(3)).Token);
                if (authenticatedUser is null) {
                    return null;
                }

                proxyServer.ServerUser = authenticatedUser;

                _proxyServers.Add(proxyServer);
                return proxyServer;
            }
            catch (Exception message) {
                return null;
            }
        }

        public void RemoveServer(ProxyServer proxyServer) {
            _proxyServers.Remove(proxyServer);
        }

        public ProxyServer? GetServer(string address) {
            return _proxyServers.SingleOrDefault(i => i.Address.Equals(address, StringComparison.OrdinalIgnoreCase));
        }

        public ProxyServer[] GetAllServers() {
            return _proxyServers.ToArray();
        }

        public ObservableCollection<ProxyServer> GetServerObservableCollection() {
            return _proxyServers;
        }
    }
}
