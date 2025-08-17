using System.Net.Sockets;

using TransparentCloudServerProxy.Managed.Models;
using TransparentCloudServerProxy.Managed.NativeC;
using TransparentCloudServerProxy.ProxyBackend.Interfaces;
using TransparentCloudServerProxy.Testables;
using TransparentCloudServerProxy.Testables.Interfaces;

namespace TransparentCloudServerProxy.ProxyBackend.NativeC {
    public class NativeCProxy : Proxy, IProxy {
        public static NativeCProxy FromInstance(IProxy instance) {
            return new(instance.PacketEngine, instance.SocketType, instance.ListenHost, instance.ListenPort, instance.TargetHost, instance.TargetPort);
        }

        private readonly List<NativeCProxyNetworkPipe> _proxyNetworkPipes = new();
        private CancellationTokenSource _cancellationTokenSource = new();

        private IProxyListener _proxyListener;
        private readonly ITestableSocketFactory _testableSocketFactory;
        private readonly IProxyListenerFactory _proxyListenerFactory;

        public NativeCProxy(PacketEngine packetEngine, ProxySocketType socketType, string listenHost, int listenPort, string targetHost, int targetPort,
            ITestableSocketFactory testableSocketFactory = null, IProxyListenerFactory proxyListenerFactory = null)
            : base(packetEngine, socketType, listenHost, listenPort, targetHost, targetPort) {
            _testableSocketFactory = testableSocketFactory;
            _proxyListenerFactory = proxyListenerFactory;

            if (_proxyListenerFactory is null) {
                _proxyListenerFactory = new ProxyListenerFactory();
            }
            if (_testableSocketFactory is null) {
                _testableSocketFactory = new TestableSocketFactory();
            }
        }

        public override bool Start() {
            _cancellationTokenSource = new();
            _proxyListener = _proxyListenerFactory.CreateProxyListener(ListenEndpoint, SocketType, _cancellationTokenSource.Token);

            _proxyListener.Start(async clientSocket => {
                try {
                    var proxyNetworkPipe = await ConnectNetworkPipe(clientSocket);
                    _proxyNetworkPipes.Add(proxyNetworkPipe);
                    proxyNetworkPipe.Start();
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
            _proxyListener?.Stop(_cancellationTokenSource);
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
            var protoType = ProtocolType.Tcp;
            var socketType = System.Net.Sockets.SocketType.Stream;

            switch (SocketType) {
                case ProxySocketType.Udp:
                    protoType = ProtocolType.Udp;
                    socketType = System.Net.Sockets.SocketType.Dgram;
                    break;
                default:
                    protoType = ProtocolType.Tcp;
                    socketType = System.Net.Sockets.SocketType.Stream;
                    break;
            }

            var targetSocket = _testableSocketFactory.CreateSocket(AddressFamily.InterNetwork, socketType, protoType);
            await targetSocket.ConnectAsync(TargetEndpoint);

            return new NativeCProxyNetworkPipe(clientSocket, targetSocket);
        }
    }
}
