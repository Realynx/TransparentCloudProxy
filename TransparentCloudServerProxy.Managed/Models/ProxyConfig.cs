using TransparentCloudServerProxy.ProxyBackend;

namespace TransparentCloudServerProxy.Managed.Models {
    public class ProxyConfig : IProxyConfig {
        public ProxyConfig() {
        }

        public Proxy[] Proxies { get; set; }
    }
}
