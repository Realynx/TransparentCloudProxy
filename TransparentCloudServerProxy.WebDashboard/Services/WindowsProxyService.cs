using TransparentCloudServerProxy.Managed;
using TransparentCloudServerProxy.WebDashboard.Models;

namespace TransparentCloudServerProxy.WebDashboard.Services {
    public class WindowsProxyService : IProxyService {
        private readonly ManagedProxyService _managedProxyService;
        private readonly ProxyConfig _proxyConfig;

        public WindowsProxyService(ProxyConfig proxyConfig) {
            _managedProxyService = new ManagedProxyService();
            _proxyConfig = proxyConfig;

            foreach (var entry in _proxyConfig.ManagedProxyEntry) {
                entry.Id = Guid.NewGuid();
                _managedProxyService.AddProxyEntry(entry);
            }

            _managedProxyService.StartAllProxies();
        }

        public void StartAllProxies() {
            _managedProxyService.StartAllProxies();
        }

        public void StartProxy(ManagedProxyEntry managedProxyEntry) {
            _managedProxyService.StartProxy(managedProxyEntry);
        }

        public void StopProxy(ManagedProxyEntry managedProxyEntry) {
            _managedProxyService.StopProxy(managedProxyEntry);
        }

        public void AddProxyEntry(ManagedProxyEntry managedProxyEntry) {
            _managedProxyService.AddProxyEntry(managedProxyEntry);
            _managedProxyService.StartProxy(managedProxyEntry);
        }

        public void RemoveProxyEntry(ManagedProxyEntry managedProxyEntry) {
            _managedProxyService.StopProxy(managedProxyEntry);
            _managedProxyService.RemoveProxyEntry(managedProxyEntry);
        }

        public ManagedProxyEntry[] GetProxies() {
            return _managedProxyService.GetProxies();
        }
    }
}
