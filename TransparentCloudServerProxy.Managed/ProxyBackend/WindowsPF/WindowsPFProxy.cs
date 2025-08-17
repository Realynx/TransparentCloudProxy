using TransparentCloudServerProxy.Managed.Interfaces;
using TransparentCloudServerProxy.Managed.Models;
using TransparentCloudServerProxy.ProxyBackend.Interfaces;
using TransparentCloudServerProxy.SystemTools;

namespace TransparentCloudServerProxy.ProxyBackend.WindowsPF {
    public class WindowsPFProxy : Proxy, IProxy {
        public static WindowsPFProxy FromInstance(IProxy instance) {
            return new(instance.PacketEngine, instance.SocketType, instance.ListenHost, instance.ListenPort, instance.TargetHost, instance.TargetPort);
        }

        public ISystemProgram Netsh { get; set; }

        public WindowsPFProxy(PacketEngine packetEngine, ProxySocketType socketType, string listenHost, int listenPort, string targetHost, int targetPort)
            : base(packetEngine, socketType, listenHost, listenPort, targetHost, targetPort) {
            if (socketType != ProxySocketType.Tcp) {
                throw new Exception("Windows PF is only capable of porxying TCP streams.");
            }

            Netsh = new Netsh();
#if !DEBUG
            if (Environment.OSVersion.Platform != PlatformID.Win32NT) {
                throw new InvalidOperationException("WindowsPF engine must be run on windows.");
            }
#endif
        }

        public override bool Start() {
            var response = Netsh.RunCommand($"interface portproxy add v4tov4 listenport={ListenPort} listenaddress={ListenHost} connectport={TargetPort} connectaddress={TargetHost} protocol=tcp");
            if (!string.IsNullOrWhiteSpace(response)) {
                return false;
            }

            Enabled = true;
            return Enabled;
        }

        public override bool Stop() {
            var response = Netsh.RunCommand($"interface portproxy delete v4tov4 listenport={ListenPort} listenaddress={ListenHost} protocol=tcp");
            if (!string.IsNullOrWhiteSpace(response)) {
                return false;
            }

            Enabled = false;
            return !Enabled;
        }
    }
}
