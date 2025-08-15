using TransparentCloudServerProxy.Managed.Interfaces;
using TransparentCloudServerProxy.Managed.Models;
using TransparentCloudServerProxy.ProxyBackend.Interfaces;
using TransparentCloudServerProxy.SystemTools;

namespace TransparentCloudServerProxy.ProxyBackend.UnixNetfilter {
    public class NetFilterProxy : Proxy, IProxy {
        public static NetFilterProxy FromInstance(IProxy instance) {
            return new(instance.PacketEngine, instance.SocketType, instance.ListenHost, instance.ListenPort, instance.TargetHost, instance.TargetPort);
        }

        public ISystemProgram NetFilterProgram { get; set; }

        public NetFilterProxy(string packetEngine, ProxySocketType socketType, string listenHost, int listenPort, string targetHost, int targetPort)
            : base(packetEngine, socketType, listenHost, listenPort, targetHost, targetPort) {
            NetFilterProgram = new NetFilter();

#if !DEBUG
            if (Environment.OSVersion.Platform != PlatformID.Unix) {
                throw new InvalidOperationException("NetFilter engine must be run on linux.");
            }
#endif
        }

        public override bool Start() {
            NetFilterProgram.RunCommand($"add {ComputeFilterRule("proxy")}");

            Enabled = true;
            return Enabled;
        }

        public override bool Stop() {
            NetFilterProgram.RunCommand($"delete {ComputeFilterRule("proxy")}");

            Enabled = false;
            return !Enabled;
        }

        private string ComputeFilterRule(string tableName, ProxySocketType? overide = null) {
            var socketType = string.Empty;

            var switchType = overide is null ? SocketType : overide;
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

            return $"rule ip {tableName} prerouting iifname != \"lo\" {socketType} dport {ListenPort} dnat to {TargetHost}:{TargetPort}";
        }
    }
}
