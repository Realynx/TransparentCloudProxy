using System.Net;

namespace TransparentCloudServerProxy.Managed.Models {
    public class ManagedProxyEntry {
        public ManagedProxyEntry(string listenAddress, int listenPort, string targetAddress, int targetPort) {
            ListenAddress = listenAddress;
            ListenPort = listenPort;

            TargetAddress = targetAddress;
            TargetPort = targetPort;
        }

        public Guid Id { get; set; }
        public bool Enabled { get; set; }
        public double MeasuredDelaynNanoSeconds { get; set; }

        public string ListenAddress { get; set; }
        public int ListenPort { get; set; }

        public string TargetAddress { get; set; }
        public int TargetPort { get; set; }

        public override string ToString() {
            return $"{ListenAddress}:{ListenPort} <-> {TargetAddress}:{TargetPort}";
        }

        public override bool Equals(object? obj) {
            if (obj is null) {
                return false;
            }

            return obj.ToString() == ToString();
        }

        public static bool operator ==(ManagedProxyEntry left, ManagedProxyEntry right) {
            return left.ToString().Equals(right.ToString(), StringComparison.OrdinalIgnoreCase);
        }

        public static bool operator !=(ManagedProxyEntry left, ManagedProxyEntry right) {
            return !left.ToString().Equals(right.ToString(), StringComparison.OrdinalIgnoreCase);
        }
    }
}
