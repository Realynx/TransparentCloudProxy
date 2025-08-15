using TransparentCloudServerProxy.Managed.Models;
using TransparentCloudServerProxy.ProxyBackend;

namespace TransparentCloudServerProxy.WebDashboard.Models {
    public class ProxyConfig : IProxyConfig {
        public ProxyConfig(IConfigurationRoot configurationRoot) {
            configurationRoot.GetSection(nameof(ProxyConfig)).Bind(this);
        }

        public Proxy[] Proxies { get; set; }
        public string PacketEngine { get; set; } = "";

    }
}
