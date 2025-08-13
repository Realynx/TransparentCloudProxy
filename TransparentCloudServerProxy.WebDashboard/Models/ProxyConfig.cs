using TransparentCloudServerProxy.Managed;

namespace TransparentCloudServerProxy.WebDashboard.Models {
    public class ProxyConfig {
        public ProxyConfig(IConfigurationRoot configurationRoot) {
            configurationRoot.GetSection(nameof(ProxyConfig)).Bind(this);
        }

        public ManagedProxyEntry[] ManagedProxyEntry { get; set; }
    }
}
