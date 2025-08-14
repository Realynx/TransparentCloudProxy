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

        public void Start() {
            // ManagedProxyEntry.Id
            var tableId = $"proxy";
            Console.WriteLine(NetFilter.RunNetFilterCommand("add rule ip " + tableId + $" prerouting iifname != \"lo\" udp dport {ManagedProxyEntry.ListenPort} dnat to {ManagedProxyEntry.TargetAddress}:{ManagedProxyEntry.TargetPort}"));
            Console.WriteLine(NetFilter.RunNetFilterCommand("add rule ip " + tableId + $" prerouting iifname != \"lo\" tcp dport {ManagedProxyEntry.ListenPort} dnat to {ManagedProxyEntry.TargetAddress}:{ManagedProxyEntry.TargetPort}"));

        }

        public void Stop() {
            var tableId = $"proxy_{ManagedProxyEntry.Id.ToString()}";

            // Console.WriteLine(NetFilter.RunNetFilterCommand($"delete table ip {tableId}"));
        }
    }
}
