using TransparentCloudServerProxy.Interfaces;
using TransparentCloudServerProxy.ProxyBackend;
using TransparentCloudServerProxy.ProxyBackend.Interfaces;
using TransparentCloudServerProxy.Services.Exceptions;

namespace TransparentCloudServerProxy.Services {
    public class ProxyService : IProxyService {
        private readonly List<IProxy> _proxies = new();
        private readonly IProxyFactory _proxyFactory;
        private readonly IPacketFilterResetService _packetFilterResetService;

        public ProxyService(IProxyFactory? proxyFactory = null, IPacketFilterResetService? packetFilterResetService = null) {
            _proxyFactory = proxyFactory ?? new ProxyFactory();
            _packetFilterResetService = packetFilterResetService ?? new PacketFilterResetService();
        }

        public void AddProxyEntry(Proxy proxy) {
            var existingProxy = _proxies.SingleOrDefault(i => (Proxy)i == proxy);
            if (existingProxy is not null) {
                throw new ProxyExistsException($"The proxy {proxy} already exists");
            }

            AddProxy(proxy);
        }

        public void AddOrUodateProxyEntry(Proxy proxy) {
            var existingProxy = _proxies.SingleOrDefault(i => (Proxy)i == proxy);
            if (existingProxy is not null) {
                existingProxy.Stop();
                _proxies.Remove(existingProxy);
            }

            AddProxy(proxy);
        }

        private void AddProxy(Proxy proxy) {
            var proxyImplementation = _proxyFactory.Create(proxy);
            _proxies.Add(proxyImplementation);

            if (proxy.Enabled) {
                _packetFilterResetService.Reset([proxyImplementation]);
                proxyImplementation.Start();
            }
        }

        public void StartAllProxies() {
            _packetFilterResetService.Reset(_proxies);

            foreach (var proxy in _proxies) {
                proxy.Start();
            }
        }

        public void StartProxy(Proxy proxy) {
            var existingProxy = FindExistingProxyOrThrow(proxy, "start");
            _packetFilterResetService.Reset([existingProxy]);
            existingProxy.Start();
        }

        public void StopProxy(Proxy proxy) {
            var existingProxy = FindExistingProxyOrThrow(proxy, "stop");
            existingProxy.Stop();
        }

        public void RemoveProxyEntry(Proxy proxy) {
            var existingProxy = FindExistingProxyOrThrow(proxy, "remove");
            existingProxy.Stop();
            existingProxy.Dispose();
            _proxies.Remove(existingProxy);
        }

        public IProxy[] GetProxies() {
            return _proxies.ToArray();
        }

        private IProxy FindExistingProxyOrThrow(Proxy proxy, string action) {
            var existingProxy = _proxies.SingleOrDefault(i => (Proxy)i == proxy);
            if (existingProxy is null) {
                throw new ProxyNotFoundException($"The proxy {proxy} could not be found to {action}");
            }

            return existingProxy;
        }
    }
}
