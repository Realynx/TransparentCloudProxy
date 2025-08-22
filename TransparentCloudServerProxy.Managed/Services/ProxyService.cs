using TransparentCloudServerProxy.ProxyBackend;
using TransparentCloudServerProxy.ProxyBackend.Interfaces;
using TransparentCloudServerProxy.ProxyBackend.Managed;
using TransparentCloudServerProxy.ProxyBackend.NativeC;
using TransparentCloudServerProxy.ProxyBackend.UnixNetfilter;
using TransparentCloudServerProxy.ProxyBackend.WindowsPF;
using TransparentCloudServerProxy.Services.Exceptions;
using TransparentCloudServerProxy.SystemTools;

namespace TransparentCloudServerProxy.Services {
    public class ProxyService : IProxyService {
        private readonly List<IProxy> _proxies = new();

        public ProxyService() {
            ResetLowLevelPacketFiltering();
        }

        private void ResetLowLevelPacketFiltering() {
            if (_proxies.Any(i => i.PacketEngine == PacketEngine.WindowsPF)) {
                new Netsh().ResetState();
            }

            if (_proxies.Any(i => i.PacketEngine == PacketEngine.NetFilter)) {
                new NetFilter().ResetTables();
            }
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
                StopProxy(existingProxy as Proxy);
                _proxies.Remove(existingProxy);

                AddProxy(proxy);
                return;
            }

            AddProxy(proxy);
        }

        private void AddProxy(Proxy proxy) {
            IProxy proxyImplementation;
            switch (proxy.PacketEngine) {
                case PacketEngine.NetFilter:
                    proxyImplementation = NetFilterProxy.FromInstance(proxy);
                    break;
                case PacketEngine.NativeC:
                    proxyImplementation = NativeCProxy.FromInstance(proxy);
                    break;
                case PacketEngine.WindowsPF:
                    proxyImplementation = WindowsPFProxy.FromInstance(proxy);
                    break;

                default:
                    proxy.PacketEngine = PacketEngine.Managed;
                    proxyImplementation = ManagedProxy.FromInstance(proxy);
                    break;
            }

            _proxies.Add(proxyImplementation);
            if (proxy.Enabled) {
                proxyImplementation.Start();
            }
        }

        public void StartAllProxies() {
            foreach (var proxy in _proxies) {
                proxy.Start();
            }
        }

        public void StartProxy(Proxy proxy) {
            var existingProxy = _proxies.SingleOrDefault(i => (Proxy)i == proxy);
            if (existingProxy is null) {
                throw new ProxyNotFoundException($"The proxy {proxy} could not be found to start");
            }

            existingProxy.Start();
        }

        public void StopProxy(Proxy proxy) {
            var existingProxy = _proxies.SingleOrDefault(i => (Proxy)i == proxy);
            if (existingProxy is null) {
                throw new ProxyNotFoundException($"The proxy {proxy} could not be found to stop");
            }

            existingProxy.Stop();
        }

        public void RemoveProxyEntry(Proxy proxy) {
            var existingProxy = _proxies.SingleOrDefault(i => (Proxy)i == proxy);
            if (existingProxy is null) {
                throw new ProxyNotFoundException($"The proxy {proxy} could not be found to stop");
            }

            existingProxy.Stop();
            existingProxy.Dispose();
            _proxies.Remove(existingProxy);
        }

        public IProxy[] GetProxies() {
            return _proxies.ToArray();
        }
    }
}
