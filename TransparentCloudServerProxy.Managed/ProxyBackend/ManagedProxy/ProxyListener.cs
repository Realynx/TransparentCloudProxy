using System.Net;
using System.Net.Sockets;

using TransparentCloudServerProxy.Managed.Models;

namespace TransparentCloudServerProxy.ProxyBackend.ManagedProxy {
    internal class ProxyListener : IDisposable {
        private readonly IPEndPoint _listenEndpoint;
        private readonly IPEndPoint _targetEndpoint;
        private readonly ProxySocketType _proxySocketType;
        private readonly CancellationToken _cancellationToken;

        private Socket _listenSocket;
        private Task _listenTask;

        public ProxyListener(IPEndPoint listenEndpoint, IPEndPoint targetEndpoint, ProxySocketType proxySocketType, CancellationToken cancellationToken) {
            _listenEndpoint = listenEndpoint;
            _targetEndpoint = targetEndpoint;
            _proxySocketType = proxySocketType;
            _cancellationToken = cancellationToken;
        }

        public void Start(Action<Socket> acceptedConnection) {
            BindSocket();

            _listenTask = Listen(acceptedConnection);
        }

        /// <summary>
        /// Cancels the token and disposes it's listner task/thread.
        /// </summary>
        /// <param name="sourceToken"></param>
        public void Stop(CancellationTokenSource sourceToken) {
            sourceToken.Cancel();

            _listenSocket.Close();
            _listenTask.Dispose();
        }

        private void BindSocket() {
            switch (_proxySocketType) {
                case ProxySocketType.Tcp:
                    _listenSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    break;
                case ProxySocketType.Udp:
                    _listenSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Udp);
                    break;
                case ProxySocketType.Any:
                    break;
                default:
                    _listenSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    break;
            }

            _listenSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            _listenSocket.Bind(_listenEndpoint);
            _listenSocket.Listen(128);
        }

        private async Task Listen(Action<Socket> acceptedConnection) {
            while (!_cancellationToken.IsCancellationRequested) {
                var clientSocket = await _listenSocket.AcceptAsync();

                acceptedConnection(clientSocket);
            }
        }

        public void Dispose() {
            _listenTask.Dispose();
            _listenSocket.Dispose();

            GC.SuppressFinalize(this);
        }
    }
}