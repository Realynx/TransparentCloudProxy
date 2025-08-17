using Microsoft.EntityFrameworkCore;

using TransparentCloudServerProxy.Managed.Models;
using TransparentCloudServerProxy.ProxyBackend;
using TransparentCloudServerProxy.ProxyBackend.Interfaces;
using TransparentCloudServerProxy.ProxyBackend.Managed;
using TransparentCloudServerProxy.ProxyBackend.NativeC;
using TransparentCloudServerProxy.ProxyBackend.UnixNetfilter;
using TransparentCloudServerProxy.ProxyBackend.WindowsPF;
using TransparentCloudServerProxy.SystemTools;
using TransparentCloudServerProxy.WebDashboard.Services.Exceptions;
using TransparentCloudServerProxy.WebDashboard.Services.Interfaces;
using TransparentCloudServerProxy.WebDashboard.SqlDb;

namespace TransparentCloudServerProxy.WebDashboard.Services {
    public class ProxyService : IProxyService {
        private readonly List<IProxy> _proxies = new();
        private readonly IProxyConfig _proxyConfig;
        private readonly IDbContextFactory<WebDashboardDbContext> _dbContextFactory;

        public ProxyService(IProxyConfig proxyConfig, IDbContextFactory<WebDashboardDbContext> dbContextFactory) {
            _proxyConfig = proxyConfig;
            _dbContextFactory = dbContextFactory;
            ResetLowLevelPacketFiltering();

            for (uint x = 0; x < proxyConfig.Proxies.Length; x++) {
                var proxy = proxyConfig.Proxies[x];
                Console.WriteLine($"[{proxy.PacketEngine}] {proxy.ToString()}");
                AddProxy(proxy);
            }

            using var dbContext = _dbContextFactory.CreateDbContext();
            var dbProxies = dbContext.Proxies.Select(i => i.GetProxy()).ToArray();
            if (dbProxies is null) {
                return;
            }

            foreach (var proxy in dbProxies) {
                if (proxy is null) {
                    continue;
                }

                Console.WriteLine($"[{proxy.PacketEngine}] {proxy.ToString()}");
                AddProxy(proxy);
            }
        }

        private void ResetLowLevelPacketFiltering() {
            if (_proxyConfig.Proxies.Any(i => i.PacketEngine == PacketEngine.WindowsPF)) {
                new Netsh().ResetState();
            }

            if (_proxyConfig.Proxies.Any(i => i.PacketEngine == PacketEngine.Netfitler)) {
                new NetFilter().ResetTables();
            }
        }

        private void AddProxy(Proxy proxy) {
            IProxy proxyImplementation;
            switch (proxy.PacketEngine) {
                case PacketEngine.Netfitler:
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
            if (proxyImplementation.Enabled) {
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

        public void AddProxyEntry(Proxy proxy) {
            var existingProxy = _proxies.SingleOrDefault(i => (Proxy)i == proxy);
            if (existingProxy is not null) {
                throw new ProxyExistsException($"The proxy {proxy} already exists");
            }

            AddProxy(proxy);
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
