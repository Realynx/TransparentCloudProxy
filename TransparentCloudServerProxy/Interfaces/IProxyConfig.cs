using TransparentCloudServerProxy.ProxyBackend;

namespace TransparentCloudServerProxy.Interfaces {
    public interface IProxyConfig {
        Proxy[] Proxies { get; set; }
    }
}