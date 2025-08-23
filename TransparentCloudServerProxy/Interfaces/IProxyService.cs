using TransparentCloudServerProxy.ProxyBackend;
using TransparentCloudServerProxy.ProxyBackend.Interfaces;

namespace TransparentCloudServerProxy.Services {
    public interface IProxyService {
        void AddOrUodateProxyEntry(Proxy proxy);
        void AddProxyEntry(Proxy proxy);
        IProxy[] GetProxies();
        void RemoveProxyEntry(Proxy proxy);
        void StartAllProxies();
        void StartProxy(Proxy proxy);
        void StopProxy(Proxy proxy);
    }
}