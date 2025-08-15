using TransparentCloudServerProxy.Managed.Models;

namespace TransparentCloudServerProxy.WebDashboard.Services.Interfaces {
    public interface IProxyService {
        void AddProxyEntry(ManagedProxyEntry managedProxyEntry);
        ManagedProxyEntry[] GetProxies();
        void RemoveProxyEntry(ManagedProxyEntry managedProxyEntry);
        void StartAllProxies();
        void StartProxy(ManagedProxyEntry managedProxyEntry);
        void StopProxy(ManagedProxyEntry managedProxyEntry);
    }
}