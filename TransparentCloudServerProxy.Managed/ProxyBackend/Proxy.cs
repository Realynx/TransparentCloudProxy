using System.Net;
using System.Text.Json.Serialization;

using TransparentCloudServerProxy.Managed.Models;
using TransparentCloudServerProxy.ProxyBackend.Exceptions;

namespace TransparentCloudServerProxy.ProxyBackend {
    public class Proxy : IProxy, IDisposable {
        [JsonIgnore]
        public IPEndPoint ListenEndpoint {
            get {
                return !IPEndPoint.TryParse($"{ListenHost}:{ListenPort}", out var endpoint)
                    ? throw new InvalidNetworkAddress($"Address: '{ListenHost}:{ListenPort}' could not be parsed.")
                    : endpoint;
            }
        }

        [JsonIgnore]
        public IPEndPoint TargetEndpoint {
            get {
                return !IPEndPoint.TryParse($"{TargetHost}:{TargetPort}", out var endpoint)
                    ? throw new InvalidNetworkAddress($"Address: '{TargetHost}:{TargetPort}' could not be parsed.")
                    : endpoint;
            }
        }

        public bool Enabled { get; set; }
        public uint Id { get; set; }

        public string ListenHost { get; init; }
        public int ListenPort { get; init; }
        public string TargetHost { get; init; }
        public int TargetPort { get; init; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public ProxySocketType SocketType { get; init; }

        public Proxy(ProxySocketType socketType, string listenHost, int listenPort, string targetHost, int targetPort) {
            SocketType = socketType;
            ListenHost = listenHost;
            ListenPort = listenPort;
            TargetHost = targetHost;
            TargetPort = targetPort;
        }

        public override string ToString() {
            return $"{ListenHost}:{ListenPort} <{SocketType}> {TargetHost}:{TargetPort}";
        }

        public virtual bool Start() {
            throw new NotImplementedException();
        }

        public virtual bool Stop() {
            throw new NotImplementedException();
        }

        public virtual void Dispose() {
        }

        public static bool operator ==(Proxy left, Proxy right) {
            return left.ToString().Equals(right.ToString(), StringComparison.OrdinalIgnoreCase);
        }

        public static bool operator !=(Proxy left, Proxy right) {
            return !(left == right);
        }
    }
}
