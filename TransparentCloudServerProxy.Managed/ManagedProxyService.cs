namespace TransparentCloudServerProxy.Managed {
    public class ManagedProxyService {
        private readonly List<ProxyEndpoint> _proxyEndpoints = new();

        public void AddProxyEntry(ManagedProxyEntry managedProxyEntry) {
            var proxyEndpoint = new ProxyEndpoint(managedProxyEntry);
            _proxyEndpoints.Add(proxyEndpoint);
        }

        public void RemoveProxyEntry(ManagedProxyEntry managedProxyEntry) {
            var proxyEndpoint = _proxyEndpoints.SingleOrDefault(i => i.ManagedProxyEntry.Equals(managedProxyEntry));
            if (proxyEndpoint is null) {
                return;
            }

            proxyEndpoint.Stop();
            proxyEndpoint.Dispose();
            _proxyEndpoints.Remove(proxyEndpoint);
        }

        public void StartAllProxies() {
            foreach (var proxyEndpoint in _proxyEndpoints) {
                proxyEndpoint.Start();
            }
        }

        public void StartProxy(ManagedProxyEntry managedProxyEntry) {
            var proxyEndpoint = _proxyEndpoints.SingleOrDefault(i => i.ManagedProxyEntry.Equals(managedProxyEntry));
            if (proxyEndpoint is null) {
                return;
            }

            proxyEndpoint.Start();
        }

        public void StopProxy(ManagedProxyEntry managedProxyEntry) {
            var proxyEndpoint = _proxyEndpoints.SingleOrDefault(i => i.ManagedProxyEntry.Equals(managedProxyEntry));
            if (proxyEndpoint is null) {
                return;
            }

            proxyEndpoint.Stop();
        }

        public ManagedProxyEntry[] GetProxies() {
            return _proxyEndpoints.Select(i => i.ManagedProxyEntry).ToArray();
        }
    }
}
