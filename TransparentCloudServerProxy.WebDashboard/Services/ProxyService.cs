using TransparentCloudServerProxy.Managed;
using TransparentCloudServerProxy.WebDashboard.Models;

namespace TransparentCloudServerProxy.WebDashboard.Services {
    public class ProxyService : IProxyService {
        private readonly ManagedProxyService _managedProxyService;
        private readonly ProxyConfig _proxyConfig;

        public ProxyService(ProxyConfig proxyConfig) {
            _managedProxyService = new ManagedProxyService();
            _proxyConfig = proxyConfig;

            foreach (var entry in _proxyConfig.ManagedProxyEntry) {
                _managedProxyService.AddProxyEntry(entry);
            }

        }

        public void StartAllProxies() {
            _managedProxyService.StartAllProxies();

        }

        public void RestartAllProxies() {

        }
    }
}
