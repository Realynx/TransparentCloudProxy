using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using TransparentCloudServerProxy.Client.Models;
using TransparentCloudServerProxy.Client.Services.Interfaces;

namespace TransparentCloudServerProxy.Client.Services.Api {
    public class ProxyServerService : IProxyServerService {

        private readonly List<ProxyServer> _proxyServers = new();
        private readonly IProxyServerFactory _proxyServerFactory;

        public ProxyServerService(IProxyServerFactory proxyServerFactory) {
            _proxyServerFactory = proxyServerFactory;
        }

        public async Task<ProxyServer?> AddServer(SavedCredential savedCredential) {
            var proxyServer = _proxyServerFactory.CreateProxyServer(savedCredential);

            if (proxyServer is null) {
                return null;
            }

            var authenticatedUser = await proxyServer.GetUser();
            if (authenticatedUser is null) {
                return null;
            }

            proxyServer.ServerUser = authenticatedUser;

            _proxyServers.Add(proxyServer);
            return proxyServer;
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
    }
}
