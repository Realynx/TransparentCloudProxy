using Microsoft.EntityFrameworkCore;

using TransparentCloudServerProxy.Interfaces;
using TransparentCloudServerProxy.ProxyBackend;
using TransparentCloudServerProxy.ProxyBackend.Interfaces;
using TransparentCloudServerProxy.Services;
using TransparentCloudServerProxy.WebDashboard.Services.Interfaces;
using TransparentCloudServerProxy.WebDashboard.SqlDb;
using TransparentCloudServerProxy.WebDashboard.SqlDb.Models;

namespace TransparentCloudServerProxy.WebDashboard.Services {
    public class DatabaseProxyService : IDatabaseProxyService {
        private readonly IDbContextFactory<WebDashboardDbContext> _dbContextFactory;
        private readonly IProxyService _proxyService;

        public DatabaseProxyService(IProxyConfig proxyConfig, IDbContextFactory<WebDashboardDbContext> dbContextFactory, IProxyService proxyService) {
            _dbContextFactory = dbContextFactory;
            _proxyService = proxyService;

            for (uint x = 0; x < proxyConfig.Proxies?.Length; x++) {
                var proxy = proxyConfig.Proxies[x];
                Console.WriteLine($"[{proxy.PacketEngine}] {proxy}");
                _proxyService.AddProxyEntry(proxy);
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

                Console.WriteLine($"[{proxy.PacketEngine}] {proxy}");
                _proxyService.AddProxyEntry(proxy);
            }
        }

        public void AddProxyEntry(Proxy proxy, Guid ownerId) {
            using var dbContext = _dbContextFactory.CreateDbContext();

            var savedProxy = new SavedProxy(proxy, ownerId);
            var existingProxy = dbContext.Proxies.Find(savedProxy.Id);
            if (existingProxy is not null) {
                _proxyService.RemoveProxyEntry(existingProxy.GetProxy()!);
                _proxyService.AddProxyEntry(proxy);

                existingProxy.SavedProxyJson = savedProxy.SavedProxyJson;
                dbContext.SaveChanges();
            }

            dbContext.Proxies.Add(savedProxy);
            dbContext.SaveChanges();

            _proxyService.AddProxyEntry(proxy);
        }

        public void StartProxy(Proxy proxy) {
            _proxyService.StartProxy(proxy);
        }

        public void StartAllProxies() {
            _proxyService.StartAllProxies();
        }

        public void StopProxy(Proxy proxy) {
            _proxyService.StopProxy(proxy);
        }

        public async Task RemoveProxyEntry(Proxy proxy, ProxyUser owner) {
            using var dbContext = _dbContextFactory.CreateDbContext();

            var currentUser = await dbContext.Users
                .Include(u => u.UserSavedProxies)
                .SingleOrDefaultAsync(u => u.Id == owner.Id);

            if (currentUser is null) {
                return;
            }

            var savedProxy = new SavedProxy(proxy, currentUser.Id);
            var existingRule = currentUser.UserSavedProxies.Single(i => i.Id == savedProxy.Id);

            _proxyService.RemoveProxyEntry(proxy);
            currentUser.UserSavedProxies.Remove(existingRule);
            dbContext.SaveChanges();
        }

        public IProxy[] GetProxies() {
            return _proxyService.GetProxies();
        }
    }
}
