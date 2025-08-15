using System.Net.Sockets;

using TransparentCloudServerProxy.Managed.Models;
using TransparentCloudServerProxy.ProxyBackend.Interfaces;
using TransparentCloudServerProxy.ProxyBackend.ManagedCode;
using TransparentCloudServerProxy.Testables;
using TransparentCloudServerProxy.Testables.Interfaces;

namespace TransparentCloudServerProxy.ProxyBackend.ManagedProxy {
    public class ManagedProxy : Proxy, IProxy, IDisposable {
        public static ManagedProxy FromInstance(IProxy instance) {
            return new(instance.SocketType, instance.ListenHost, instance.ListenPort, instance.TargetHost, instance.TargetPort);
        }

        private readonly List<ProxyNetworkPipe> _proxyNetworkPipes = new();
        private CancellationTokenSource _cancellationTokenSource = new();

        private IProxyListener _proxyListener;
        private readonly ITestableSocketFactory _testableSocketFactory;
        private readonly IProxyListenerFactory _proxyListenerFactory;

        public ManagedProxy(ProxySocketType socketType, string listenHost, int listenPort, string targetHost, int targetPort,
            ITestableSocketFactory testableSocketFactory = null, IProxyListenerFactory proxyListenerFactory = null)
            : base(socketType, listenHost, listenPort, targetHost, targetPort) {
            _testableSocketFactory = testableSocketFactory;
            _proxyListenerFactory = proxyListenerFactory;
        }

        public override bool Start() {
            _cancellationTokenSource = new();
            _proxyListener = _proxyListenerFactory.CreateProxyListener(ListenEndpoint, SocketType, _cancellationTokenSource.Token);

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

        private async Task<ProxyNetworkPipe> ConnectNetworkPipe(ITestableSocket clientSocket) {
            var protoType = ProtocolType.Tcp;
            switch (SocketType) {
                case ProxySocketType.Udp:
                    protoType = ProtocolType.Udp;
                    break;
                default:
                    protoType = ProtocolType.Tcp;
                    break;
            }

            var targetSocket = _testableSocketFactory.CreateSocket(AddressFamily.InterNetwork, System.Net.Sockets.SocketType.Stream, protoType);
            await targetSocket.ConnectAsync(TargetEndpoint);
            return new ProxyNetworkPipe(clientSocket, targetSocket);
        }
    }
}
