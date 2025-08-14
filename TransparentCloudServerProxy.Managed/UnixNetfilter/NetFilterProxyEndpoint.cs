using TransparentCloudServerProxy.Managed.Interfaces;
using TransparentCloudServerProxy.Managed.Models;
using TransparentCloudServerProxy.Managed.UnixNetfilter.IpTablesApi;

namespace TransparentCloudServerProxy.Managed.UnixNetfilter {
    public class NetFilterProxyEndpoint : IDisposable, IProxyEndpoint {
        public ManagedProxyEntry ManagedProxyEntry { get; }

        public NetFilterProxyEndpoint(ManagedProxyEntry managedProxyEntry) {
            ManagedProxyEntry = managedProxyEntry;
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
            if (ManagedProxyEntry.ProxySocketType != ProxySocketType.Tcp_Udp) {
                Console.WriteLine(NetFilter.RunNetFilterCommand($"add {ComputeFilterRule("proxy")}"));
                return;
            }

            Console.WriteLine(NetFilter.RunNetFilterCommand($"add {ComputeFilterRule("proxy", ProxySocketType.Tcp)}"));
            Console.WriteLine(NetFilter.RunNetFilterCommand($"add {ComputeFilterRule("proxy", ProxySocketType.Udp)}"));
        }

        public void Stop() {
            if (ManagedProxyEntry.ProxySocketType != ProxySocketType.Tcp_Udp) {
                Console.WriteLine(NetFilter.RunNetFilterCommand($"delete {ComputeFilterRule("proxy")}"));
                return;
            }

            Console.WriteLine(NetFilter.RunNetFilterCommand($"delete {ComputeFilterRule("proxy", ProxySocketType.Tcp)}"));
            Console.WriteLine(NetFilter.RunNetFilterCommand($"delete {ComputeFilterRule("proxy", ProxySocketType.Udp)}"));
        }
    }
}
