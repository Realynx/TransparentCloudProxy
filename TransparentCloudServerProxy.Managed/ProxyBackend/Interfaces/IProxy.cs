using System.Text.Json.Serialization;

using TransparentCloudServerProxy.Managed.Models;

namespace TransparentCloudServerProxy.ProxyBackend.Interfaces {
    public interface IProxy {
        string ListenHost { get; init; }
        int ListenPort { get; init; }
        string TargetHost { get; init; }
        int TargetPort { get; init; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        ProxySocketType SocketType { get; init; }
        PacketEngine PacketEngine { get; set; }
        bool Enabled { get; set; }
        void Dispose();
        bool Start();
        bool Stop();
    }
}
