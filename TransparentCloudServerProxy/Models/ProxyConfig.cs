using TransparentCloudServerProxy.Interfaces;
using TransparentCloudServerProxy.ProxyBackend;

namespace TransparentCloudServerProxy.Models {
    public class ProxyConfig : IProxyConfig {
        public ProxyConfig() {
        }

        public Proxy[] Proxies { get; set; }
    }
}
