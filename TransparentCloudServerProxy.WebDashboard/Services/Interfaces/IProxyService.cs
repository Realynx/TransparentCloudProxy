using TransparentCloudServerProxy.ProxyBackend;
using TransparentCloudServerProxy.ProxyBackend.Interfaces;

namespace TransparentCloudServerProxy.WebDashboard.Services.Interfaces {
    public interface IProxyService {
        void AddProxyEntry(Proxy managedProxyEntry);
        IProxy[] GetProxies();
        void RemoveProxyEntry(Proxy managedProxyEntry);
        void StartAllProxies();
        void StartProxy(Proxy managedProxyEntry);
        void StopProxy(Proxy managedProxyEntry);
    }
}