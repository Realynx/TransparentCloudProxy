using TransparentCloudServerProxy.Managed;

namespace TransparentCloudServerProxy.WebDashboard.Services {
    public interface IProxyService {
        void AddProxyEntry(ManagedProxyEntry managedProxyEntry);
        void StartAllProxies();
        void StartProxy(ManagedProxyEntry managedProxyEntry);
        void StopProxy(ManagedProxyEntry managedProxyEntry);
    }
}