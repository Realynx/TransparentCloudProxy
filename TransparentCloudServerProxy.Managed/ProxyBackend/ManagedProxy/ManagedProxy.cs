using System.Net.Sockets;

using TransparentCloudServerProxy.Managed.Models;
using TransparentCloudServerProxy.ProxyBackend.ManagedCode;

namespace TransparentCloudServerProxy.ProxyBackend.ManagedProxy {
    public class ManagedProxy : Proxy, IProxy, IDisposable {
        public static ManagedProxy FromInstance(IProxy instance) {
            return new(instance.SocketType, instance.ListenHost, instance.ListenPort, instance.TargetHost, instance.TargetPort);
        }

        private readonly List<ProxyNetworkPipe> _proxyNetworkPipes = new();
        private CancellationTokenSource _cancellationTokenSource = new();

        private ProxyListener _proxyListener;

        public ManagedProxy(ProxySocketType socketType, string listenHost, int listenPort, string targetHost, int targetPort)
            : base(socketType, listenHost, listenPort, targetHost, targetPort) {
        }

        public override bool Start() {
            _cancellationTokenSource = new();
            _proxyListener = new ProxyListener(ListenEndpoint, TargetEndpoint, SocketType, _cancellationTokenSource.Token);

            _proxyListener.Start(async clientSocket => {
                try {
                    var proxyNetworkPipe = await ConnectNetworkPipe(clientSocket);
                    _proxyNetworkPipes.Add(proxyNetworkPipe);
                }
                catch {
                    clientSocket.Close();
                    clientSocket.Dispose();
                }
            });

            Enabled = true;
            return Enabled;
        }

        public override void Dispose() {
            _proxyListener.Dispose();
        }

        public override bool Stop() {
            _proxyListener.Stop(_cancellationTokenSource);
            DisposeProxyPipes();

            Enabled = false;
            return Enabled;
        }

        private void DisposeProxyPipes() {
            foreach (var networkPipe in _proxyNetworkPipes) {
                networkPipe.Stop();
                networkPipe.Dispose();
            }
            _proxyNetworkPipes.Clear();
        }

        private async Task<ProxyNetworkPipe> ConnectNetworkPipe(Socket clientSocket) {
            Socket? targetSocket = null;
            switch (SocketType) {
                case ProxySocketType.Tcp:
                    targetSocket = new Socket(AddressFamily.InterNetwork, System.Net.Sockets.SocketType.Stream, ProtocolType.Tcp);
                    break;
                case ProxySocketType.Udp:
                    targetSocket = new Socket(AddressFamily.InterNetwork, System.Net.Sockets.SocketType.Stream, ProtocolType.Udp);
                    break;
                case ProxySocketType.Any:
                    break;
                default:
                    targetSocket = new Socket(AddressFamily.InterNetwork, System.Net.Sockets.SocketType.Stream, ProtocolType.Tcp);
                    break;
            }

            if (targetSocket is null) {
                throw new Exception("Could not connect to target socket.");
            }

            await targetSocket.ConnectAsync(TargetEndpoint);
            return new ProxyNetworkPipe(clientSocket, targetSocket);
        }
    }
}
