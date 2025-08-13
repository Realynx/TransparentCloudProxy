using TransparentCloudServerProxy.Managed.Interfaces;
using TransparentCloudServerProxy.Managed.ManagedCode;
using TransparentCloudServerProxy.Managed.Models;
using TransparentCloudServerProxy.Managed.NativeC;
using TransparentCloudServerProxy.WebDashboard.Models;

namespace TransparentCloudServerProxy.WebDashboard.Services {
    public class WindowsProxyService : IProxyService {
        private readonly List<IProxyEndpoint> _proxyEndpoints = new();
        private readonly ProxyConfig _proxyConfig;

        public WindowsProxyService(ProxyConfig proxyConfig) {
            _proxyConfig = proxyConfig;

            foreach (var entry in _proxyConfig.ManagedProxyEntry) {
                entry.Id = Guid.NewGuid();
                AddProxyEntry(entry);
            }
        }

        public void StartAllProxies() {
            foreach (var proxy in _proxyEndpoints) {
                proxy.Start();
            }
        }

        public void StartProxy(ManagedProxyEntry managedProxyEntry) {
            var existingProxy = _proxyEndpoints.SingleOrDefault(i => i.ManagedProxyEntry == managedProxyEntry);
            if (existingProxy is null) {
                return;
            }

            managedProxyEntry.Enabled = true;
            existingProxy.Start();
        }

        public void StopProxy(ManagedProxyEntry managedProxyEntry) {
            var existingProxy = _proxyEndpoints.SingleOrDefault(i => i.ManagedProxyEntry == managedProxyEntry);
            if (existingProxy is null) {
                return;
            }

            existingProxy.Stop();
            managedProxyEntry.Enabled = false;
        }

        public void AddProxyEntry(ManagedProxyEntry managedProxyEntry) {
            var existingProxy = _proxyEndpoints.SingleOrDefault(i => i.ManagedProxyEntry == managedProxyEntry);
            if (existingProxy is not null) {
                return;
            }

            switch (_proxyConfig.PacketEngine) {
                case "NativeC":
                    var nativeProxyEndpoint = new NativeCProxyEndpoint(managedProxyEntry);
                    _proxyEndpoints.Add(nativeProxyEndpoint);
                    nativeProxyEndpoint.Start();
                    break;
                case "NativeR":

                    break;
                default:
                    var managedProxyEndpoint = new ManagedProxyEndpoint(managedProxyEntry);
                    _proxyEndpoints.Add(managedProxyEndpoint);
                    managedProxyEndpoint.Start();
                    break;
            }

            managedProxyEntry.Enabled = true;
        }

        public void RemoveProxyEntry(ManagedProxyEntry managedProxyEntry) {
            var existingProxy = _proxyEndpoints.SingleOrDefault(i => i.ManagedProxyEntry == managedProxyEntry);
            if (existingProxy is null) {
                return;
            }

            existingProxy.Stop();
            existingProxy.Dispose();
            _proxyEndpoints.Remove(existingProxy);
        }

        public ManagedProxyEntry[] GetProxies() {
            foreach (var proxy in _proxyEndpoints) {
                proxy.ManagedProxyEntry.MeasuredDelaynNanoSeconds = proxy.GetAverageDelayNanoSecond();
            }

            return _proxyEndpoints.Select(i => i.ManagedProxyEntry).ToArray();
        }
    }
}
