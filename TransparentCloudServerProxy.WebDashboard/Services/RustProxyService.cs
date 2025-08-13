using TransparentCloudServerProxy.WebDashboard.Models;

namespace TransparentCloudServerProxy.WebDashboard.Services {
    public class RustProxyService : IProxyService {
        private readonly ProxyConfig _proxyConfig;

        public RustProxyService(ProxyConfig proxyConfig) {
            _proxyConfig = proxyConfig;

            foreach (var entry in _proxyConfig.ManagedProxyEntry) {

            }
        }

        public void RestartAllProxies() {
            throw new NotImplementedException();
        }

        public void StartAllProxies() {
            throw new NotImplementedException();
        }
    }
}
