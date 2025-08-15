using TransparentCloudServerProxy.ProxyBackend;

namespace TransparentCloudServerProxy.WebDashboard.Services.Interfaces {
    public interface IProxyService {
        void AddProxyEntry(Proxy managedProxyEntry);
        Proxy[] GetProxies();
        void RemoveProxyEntry(Proxy managedProxyEntry);
        void StartAllProxies();
        void StartProxy(Proxy managedProxyEntry);
        void StopProxy(Proxy managedProxyEntry);
    }
}