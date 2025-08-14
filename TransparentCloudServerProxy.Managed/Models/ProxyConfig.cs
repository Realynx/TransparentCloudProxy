namespace TransparentCloudServerProxy.Managed.Models {
    public class ProxyConfig : IProxyConfig {
        public ProxyConfig() {
        }

        public ManagedProxyEntry[] ManagedProxyEntry { get; set; }
        public string PacketEngine { get; set; } = "";

    }
}
