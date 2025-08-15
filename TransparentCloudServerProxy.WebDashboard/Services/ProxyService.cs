using TransparentCloudServerProxy.Managed.Models;
using TransparentCloudServerProxy.ProxyBackend;
using TransparentCloudServerProxy.ProxyBackend.Interfaces;
using TransparentCloudServerProxy.ProxyBackend.NativeC;
using TransparentCloudServerProxy.ProxyBackend.WindowsPF;
using TransparentCloudServerProxy.SystemTools;
using TransparentCloudServerProxy.WebDashboard.Services.Exceptions;
using TransparentCloudServerProxy.WebDashboard.Services.Interfaces;

namespace TransparentCloudServerProxy.WebDashboard.Services {
    public class ProxyService : IProxyService {
        private readonly List<IProxy> _proxies = new();
        private readonly IProxyConfig _proxyConfig;

        public ProxyService(IProxyConfig proxyConfig) {
            _proxyConfig = proxyConfig;
            ResetLowLevelPacketFiltering();

            for (uint x = 0; x < proxyConfig.Proxies.Length; x++) {
                var proxy = proxyConfig.Proxies[x];
                Console.WriteLine($"[{proxy.PacketEngine}] {proxy.ToString()}");
                AddStartProxy(proxy);
            }
        }

        private void ResetLowLevelPacketFiltering() {
            if (_proxyConfig.Proxies.Any(i => i.PacketEngine == "WindowsPF")) {
                new Netsh().ResetState();
            }

            if (_proxyConfig.Proxies.Any(i => i.PacketEngine == "NetFilter")) {
                new NetFilter().ResetTables();
            }
        }

        private void AddStartProxy(Proxy proxy) {
            IProxy proxyImplementation;
            switch (proxy.PacketEngine) {
                case "NetFilter":
                    proxyImplementation = NativeCProxy.FromInstance(proxy);
                    break;
                case "NativeC":
                    proxyImplementation = NativeCProxy.FromInstance(proxy);
                    break;
                case "WindowsPF":
                    proxyImplementation = WindowsPFProxy.FromInstance(proxy);
                    break;

                default:
                    proxy.PacketEngine = "Managed";
                    proxyImplementation = NativeCProxy.FromInstance(proxy);
                    break;
            }

            _proxies.Add(proxyImplementation);
            proxyImplementation.Start();
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

        public void AddProxyEntry(Proxy proxy) {
            var existingProxy = _proxies.SingleOrDefault(i => (Proxy)i == proxy);
            if (existingProxy is not null) {
                throw new ProxyExistsException($"The proxy {proxy} already exists");
            }

            AddStartProxy(proxy);
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
