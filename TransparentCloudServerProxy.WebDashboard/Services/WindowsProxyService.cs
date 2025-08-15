using TransparentCloudServerProxy.Managed.Interfaces;
using TransparentCloudServerProxy.Managed.ManagedCode;
using TransparentCloudServerProxy.Managed.Models;
using TransparentCloudServerProxy.Managed.NativeC;
using TransparentCloudServerProxy.Managed.UnixNetfilter;
using TransparentCloudServerProxy.WebDashboard.Services.Interfaces;

namespace TransparentCloudServerProxy.WebDashboard.Services {
    public class WindowsProxyService : IProxyService {
        private readonly List<IProxyEndpoint> _proxyEndpoints = new();
        private readonly IProxyConfig _proxyConfig;

        public WindowsProxyService(IProxyConfig proxyConfig) {
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
                case "NetFilter":
                    var netFilterEndpoint = new NetFilterProxyEndpoint(managedProxyEntry);
                    _proxyEndpoints.Add(netFilterEndpoint);
                    netFilterEndpoint.Start();
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
                proxy.ManagedProxyEntry.MeasuredDelayNanoSeconds = proxy.GetAverageDelayNanoSecond();
            }

            return _proxyEndpoints.Select(i => i.ManagedProxyEntry).ToArray();
        }
    }
}
