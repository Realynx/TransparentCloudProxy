using TransparentCloudServerProxy.Bindings.ManagedProxy;

namespace TransparentCloudServerProxy.Cli.Models {
    public class ProxyConfig {
        public ManagedProxyEntry[] ManagedProxyEntry { get; set; } = Array.Empty<ManagedProxyEntry>();
    }
}
