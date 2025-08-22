using System;
using System.Threading;
using System.Threading.Tasks;

using TransparentCloudServerProxy.Client.Models;
using TransparentCloudServerProxy.ProxyBackend;
using TransparentCloudServerProxy.ProxyBackend.Interfaces;
using TransparentCloudServerProxy.Services;
using TransparentCloudServerProxy.SystemTools;
using TransparentCloudServerProxy.WebDashboard.SqlDb.Models;

namespace TransparentCloudServerProxy.Client.Services.Api {
    public class LocalProxyServer : IProxyServer {
        private readonly IProxyService _proxyService;

        public LocalProxyServer(IProxyService proxyService) {
            ResetLowLevelPacketFiltering();
            _proxyService = proxyService;
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
            _proxyService.RemoveProxyEntry(proxy);
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
            _proxyService.AddOrUodateProxyEntry(proxy);
            return Task.FromResult(true);
        }

        public IProxy[] GetProxies() {
            return _proxyService.GetProxies();
        }

        private void ResetLowLevelPacketFiltering() {
            try {
                new Netsh().ResetState();
            }
            catch {

            }

            try {
                new NetFilter().ResetTables();
            }
            catch {

            }
        }
    }
}
