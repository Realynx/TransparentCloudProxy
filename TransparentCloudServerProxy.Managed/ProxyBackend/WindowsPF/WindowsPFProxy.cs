using TransparentCloudServerProxy.Managed.Interfaces;
using TransparentCloudServerProxy.Managed.Models;
using TransparentCloudServerProxy.ProxyBackend.Interfaces;
using TransparentCloudServerProxy.SystemTools;

namespace TransparentCloudServerProxy.ProxyBackend.WindowsPF {
    public class WindowsPFProxy : Proxy, IProxy {
        public static WindowsPFProxy FromInstance(IProxy instance) {
            return new(instance.SocketType, instance.ListenHost, instance.ListenPort, instance.TargetHost, instance.TargetPort);
        }

        public ISystemProgram Netsh { get; set; }

        public WindowsPFProxy(ProxySocketType socketType, string listenHost, int listenPort, string targetHost, int targetPort)
            : base(socketType, listenHost, listenPort, targetHost, targetPort) {
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
            var response = Netsh.RunCommand($"interface portproxy add {ComputeFilterRule()}");
            if (!string.IsNullOrWhiteSpace(response)) {
                return false;
            }

            Enabled = true;
            return Enabled;
        }

        public override bool Stop() {
            var response = Netsh.RunCommand($"interface portproxy delete {ComputeFilterRule()}");
            if (!string.IsNullOrWhiteSpace(response)) {
                return false;
            }

            Enabled = false;
            return !Enabled;
        }

        private string ComputeFilterRule() {
            return $"v4tov4 listenport={ListenPort} listenaddress={ListenHost} connectport={TargetPort} connectaddress={TargetHost} protocol=tcp";
        }
    }
}
