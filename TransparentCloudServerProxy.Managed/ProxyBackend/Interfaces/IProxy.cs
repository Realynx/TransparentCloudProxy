using TransparentCloudServerProxy.Managed.Models;

namespace TransparentCloudServerProxy.ProxyBackend.Interfaces {
    public interface IProxy {
        string ListenHost { get; init; }
        int ListenPort { get; init; }
        string TargetHost { get; init; }
        int TargetPort { get; init; }
        ProxySocketType SocketType { get; init; }
        string PacketEngine { get; set; }
        bool Enabled { get; set; }
        uint Id { get; set; }

        void Dispose();
        bool Start();
        bool Stop();
    }
}
