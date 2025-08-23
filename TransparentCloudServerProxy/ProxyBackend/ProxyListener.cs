using System.Net;
using System.Net.Sockets;

using TransparentCloudServerProxy.Models;
using TransparentCloudServerProxy.ProxyBackend.Interfaces;
using TransparentCloudServerProxy.Testables;
using TransparentCloudServerProxy.Testables.Interfaces;

namespace TransparentCloudServerProxy.ProxyBackend {
    public class ProxyListener : IDisposable, IProxyListener {
        private readonly IPEndPoint _listenEndpoint;
        private readonly ProxySocketType _proxySocketType;
        private readonly CancellationToken _cancellationToken;

        private readonly ITestableSocketFactory _testableSocketFactory;
        private ITestableSocket _listenSocket;
        private Task _listenTask;

        public ProxyListener(IPEndPoint listenEndpoint, ProxySocketType proxySocketType, CancellationToken cancellationToken, ITestableSocketFactory testableSocketFactory = null) {
            _listenEndpoint = listenEndpoint;
            _proxySocketType = proxySocketType;
            _cancellationToken = cancellationToken;

            _testableSocketFactory = testableSocketFactory;
            if (_testableSocketFactory is null) {
                _testableSocketFactory = new TestableSocketFactory();
            }
        }

        public void Start(Action<ITestableSocket> acceptedConnection) {
            BindSocket();

            _listenTask = Task.Run(() => Listen(acceptedConnection));
        }

        /// <summary>
        /// Cancels the token and disposes it's listner task/thread.
        /// </summary>
        /// <param name="sourceToken"></param>
        public void Stop(CancellationTokenSource sourceToken) {
            sourceToken.Cancel();
            _listenSocket.Close();

            Dispose();
        }

        private void BindSocket() {
            var protoType = ProtocolType.Tcp;
            var socketType = SocketType.Stream;

            switch (_proxySocketType) {
                case ProxySocketType.Udp:
                    protoType = ProtocolType.Udp;
                    socketType = SocketType.Dgram;
                    break;
                default:
                    protoType = ProtocolType.Tcp;
                    socketType = SocketType.Stream;
                    break;
            }

            _listenSocket = _testableSocketFactory.CreateSocket(AddressFamily.InterNetwork, socketType, protoType);
            _listenSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            _listenSocket.Bind(_listenEndpoint);
            _listenSocket.Listen(128);
        }

        private async Task Listen(Action<TestableSocket> acceptedConnection) {
            while (!_cancellationToken.IsCancellationRequested) {
                var clientSocket = await _listenSocket.AcceptAsync();

                acceptedConnection(clientSocket);
            }
        }

        public void Dispose() {
            while (!_listenTask.IsCompleted) {
            }

            _listenTask.Dispose();
            _listenSocket.Dispose();

            GC.SuppressFinalize(this);
        }
    }
}