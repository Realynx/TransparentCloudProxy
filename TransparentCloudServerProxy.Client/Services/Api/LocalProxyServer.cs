using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using TransparentCloudServerProxy.Client.Models;
using TransparentCloudServerProxy.ProxyBackend;
using TransparentCloudServerProxy.ProxyBackend.Interfaces;
using TransparentCloudServerProxy.SystemTools;
using TransparentCloudServerProxy.WebDashboard.SqlDb.Models;

namespace TransparentCloudServerProxy.Client.Services.Api {
    public class LocalProxyServer : IProxyServer {
        private readonly List<IProxy> _proxies = new();
        public LocalProxyServer() {
            ResetLowLevelPacketFiltering();
        }

        public string Address {
            get {
                return "Local";
            }
        }

        public SavedCredential SavedCredential {
            get {
                return new SavedCredential() {
                    ReachableAddress = "Local",
                    Credential = "Local"
                };
            }
        }

        public ProxyUser ServerUser { get; set; }

        public Task<bool> DeleteProxy(Proxy proxy, CancellationToken cancellationToken = default) {
            if (!_proxies.Contains(proxy)) {
                return Task.FromResult(false);
            }

            _proxies.Remove(proxy);
            return Task.FromResult(true);
        }

        public Task<ProxyUser?> GetUser(CancellationToken cancellationToken = default) {
            var localUser = new ProxyUser() {
                Admin = true,
                Id = Guid.Empty,
                LastLogin = DateTime.Now,
                Username = "Local User",
                UserSavedProxies = new(),
                HashedCredentialKey = string.Empty
            };

            return Task.FromResult(localUser);
        }

        public Task<bool> UpdateOrAddProxy(Proxy proxy, CancellationToken cancellationToken = default) {
            if (_proxies.Contains(proxy)) {
                _proxies.Remove(proxy);
                _proxies.Add(proxy);
                return Task.FromResult(true);
            }

            _proxies.Add(proxy);
            return Task.FromResult(true);
        }

        private void ResetLowLevelPacketFiltering() {
            if (_proxies.Any(i => i.PacketEngine == PacketEngine.WindowsPF)) {
                new Netsh().ResetState();
            }

            if (_proxies.Any(i => i.PacketEngine == PacketEngine.NetFilter)) {
                new NetFilter().ResetTables();
            }
        }
    }
}
