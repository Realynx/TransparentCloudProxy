using TransparentCloudServerProxy.ProxyBackend;
using TransparentCloudServerProxy.ProxyBackend.Interfaces;
using TransparentCloudServerProxy.WebDashboard.SqlDb.Models;

namespace TransparentCloudServerProxy.WebDashboard.Services.Interfaces {
    public interface IDatabaseProxyService {
        void AddProxyEntry(Proxy proxy, Guid ownerId);
        IProxy[] GetProxies();
        Task RemoveProxyEntry(Proxy proxy, ProxyUser owner);
        void StartAllProxies();
        void StartProxy(Proxy proxy);
        void StopProxy(Proxy proxy);
    }
}