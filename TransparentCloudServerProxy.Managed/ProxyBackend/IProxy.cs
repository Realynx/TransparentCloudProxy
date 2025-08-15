using TransparentCloudServerProxy.Managed.Models;

namespace TransparentCloudServerProxy.ProxyBackend {
    public interface IProxy {
        string ListenHost { get; init; }
        int ListenPort { get; init; }
        string TargetHost { get; init; }
        int TargetPort { get; init; }
        ProxySocketType SocketType { get; init; }

        void Dispose();
        bool Start();
        bool Stop();
    }
}
