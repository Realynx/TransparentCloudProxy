using System.Net.Sockets;

using TransparentCloudServerProxy.Managed.Models;
using TransparentCloudServerProxy.Managed.NativeC;
using TransparentCloudServerProxy.ProxyBackend.Interfaces;
using TransparentCloudServerProxy.Testables;
using TransparentCloudServerProxy.Testables.Interfaces;

namespace TransparentCloudServerProxy.ProxyBackend.NativeCProxy {
    public class NativeCProxy : Proxy, IProxy {
        public static NativeCProxy FromInstance(IProxy instance) {
            return new(instance.SocketType, instance.ListenHost, instance.ListenPort, instance.TargetHost, instance.TargetPort);
        }

        private readonly List<NativeCProxyNetworkPipe> _proxyNetworkPipes = new();
        private CancellationTokenSource _cancellationTokenSource = new();

        private ProxyListener _proxyListener;

        public NativeCProxy(ProxySocketType socketType, string listenHost, int listenPort, string targetHost, int targetPort)
            : base(socketType, listenHost, listenPort, targetHost, targetPort) {
        }

        public override bool Start() {
            _cancellationTokenSource = new();
            _proxyListener = new ProxyListener(ListenEndpoint, SocketType, _cancellationTokenSource.Token);

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

        private async Task<NativeCProxyNetworkPipe> ConnectNetworkPipe(ITestableSocket clientSocket) {
            var targetSocket = new TestableSocket(AddressFamily.InterNetwork, System.Net.Sockets.SocketType.Stream, ProtocolType.Tcp);
            await targetSocket.ConnectAsync(TargetEndpoint);

            return new NativeCProxyNetworkPipe(clientSocket, targetSocket);
        }
    }
}
