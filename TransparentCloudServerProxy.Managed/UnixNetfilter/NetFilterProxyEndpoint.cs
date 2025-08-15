using TransparentCloudServerProxy.Managed.Interfaces;
using TransparentCloudServerProxy.Managed.Models;

namespace TransparentCloudServerProxy.Managed.UnixNetfilter {
    public class NetFilterProxyEndpoint : IDisposable, IProxyEndpoint {
        private readonly ISystemProgram _netFilterProgram;

        public ManagedProxyEntry ManagedProxyEntry { get; }

        public NetFilterProxyEndpoint(ManagedProxyEntry managedProxyEntry, ISystemProgram netFilterProgram) {
            ManagedProxyEntry = managedProxyEntry;
            _netFilterProgram = netFilterProgram;
        }

        public override string ToString() {
            return $"{ManagedProxyEntry.ListenAddress}:{ManagedProxyEntry.ListenPort} <-> {ManagedProxyEntry.TargetAddress}:{ManagedProxyEntry.TargetPort}";
        }

        public void Dispose() {

        }

        public double GetAverageDelayNanoSecond() {
            return 0;
        }

        private string ComputeFilterRule(string tableName, ProxySocketType? overide = null) {
            var socketType = string.Empty;

            var switchType = overide is null ? ManagedProxyEntry.ProxySocketType : overide;
            switch (switchType) {
                case ProxySocketType.Tcp:
                    socketType = "tcp";
                    break;
                case ProxySocketType.Udp:
                    socketType = "udp";
                    break;
                case ProxySocketType.Any:
                    socketType = string.Empty;
                    break;

                default:
                    socketType = "tcp";
                    break;
            }

            return $"rule ip {tableName} prerouting iifname != \"lo\" {socketType} dport {ManagedProxyEntry.ListenPort} dnat to {ManagedProxyEntry.TargetAddress}:{ManagedProxyEntry.TargetPort}";
        }

        public void Start() {
            _netFilterProgram.RunCommand($"add {ComputeFilterRule("proxy")}");
        }

        public void Stop() {
            _netFilterProgram.RunCommand($"delete {ComputeFilterRule("proxy")}");
        }
    }
}
