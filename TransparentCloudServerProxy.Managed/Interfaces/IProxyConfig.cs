using TransparentCloudServerProxy.ProxyBackend;

namespace TransparentCloudServerProxy.Managed.Models {
    public interface IProxyConfig {
        Proxy[] Proxies { get; set; }
        string PacketEngine { get; set; }
    }
}