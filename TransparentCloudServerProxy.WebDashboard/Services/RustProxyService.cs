using TransparentCloudServerProxy.Managed.Models;
using TransparentCloudServerProxy.WebDashboard.Models;

namespace TransparentCloudServerProxy.WebDashboard.Services {
    public class RustProxyService : IProxyService {
        private readonly ProxyConfig _proxyConfig;

        public RustProxyService(ProxyConfig proxyConfig) {
            _proxyConfig = proxyConfig;

            foreach (var entry in _proxyConfig.ManagedProxyEntry) {

            }
        }

        public void AddProxyEntry(ManagedProxyEntry managedProxyEntry) {
            throw new NotImplementedException();
        }

        public ManagedProxyEntry[] GetProxies() {
            throw new NotImplementedException();
        }

        public void RemoveProxyEntry(ManagedProxyEntry managedProxyEntry) {
            throw new NotImplementedException();
        }

        public void StartAllProxies() {
            throw new NotImplementedException();
        }

        public void StartProxy(ManagedProxyEntry managedProxyEntry) {
            throw new NotImplementedException();
        }

        public void StopProxy(ManagedProxyEntry managedProxyEntry) {
            throw new NotImplementedException();
        }
    }
}
