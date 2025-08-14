namespace TransparentCloudServerProxy.Managed.Models {
    public interface IProxyConfig {
        ManagedProxyEntry[] ManagedProxyEntry { get; set; }
        string PacketEngine { get; set; }
    }
}