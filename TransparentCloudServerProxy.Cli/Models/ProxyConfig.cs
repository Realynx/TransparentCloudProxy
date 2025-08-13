using TransparentCloudServerProxy.Managed.Models;

namespace TransparentCloudServerProxy.Cli.Models {
    public class ProxyConfig {
        public ManagedProxyEntry[] ManagedProxyEntry { get; set; } = Array.Empty<ManagedProxyEntry>();
        public string PacketEngine { get; set; } = "";
    }
}
