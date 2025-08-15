using System.Net;
using System.Net.Sockets;

using TransparentCloudServerProxy.Managed.Interfaces;
using TransparentCloudServerProxy.Managed.Models;

namespace TransparentCloudServerProxy.Managed.ManagedCode {
    public class ManagedProxyEndpoint : IDisposable, IProxyEndpoint {
        public ManagedProxyEntry ManagedProxyEntry { get; }

        private readonly IPEndPoint _targetEndpoint;
        private readonly List<ProxyNetworkPipe> _proxyNetworkPipes = new();

        private Socket _listenSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        private CancellationTokenSource _cancellationTokenSource = new();

        public ManagedProxyEndpoint(ManagedProxyEntry managedProxyEntry) {
            ManagedProxyEntry = managedProxyEntry;

            _targetEndpoint = new IPEndPoint(IPAddress.Parse(ManagedProxyEntry.TargetAddress), ManagedProxyEntry.TargetPort);
        }

        public override string ToString() {
            return ManagedProxyEntry.ToString();
        }

        public void Start() {
            _cancellationTokenSource = new();
            BindSocket();

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            Listen();
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

            ManagedProxyEntry.Enabled = true;
        }

        public void Stop() {
            _cancellationTokenSource.Cancel();

            foreach (var networkPipe in _proxyNetworkPipes) {
                networkPipe.Stop();
            }
            _proxyNetworkPipes.Clear();

            _listenSocket.Close();
            ManagedProxyEntry.Enabled = false;
        }

        private void BindSocket() {
            switch (ManagedProxyEntry.ProxySocketType) {
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

            var listenEndpoint = new IPEndPoint(IPAddress.Parse(ManagedProxyEntry.ListenAddress), ManagedProxyEntry.ListenPort);
            _listenSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            _listenSocket.Bind(listenEndpoint);
            _listenSocket.Listen(128);

            Console.WriteLine($"Listening on: {listenEndpoint}");
        }

        private async Task Listen() {
            while (!_cancellationTokenSource.IsCancellationRequested) {
                PruneNetworkPipes();

                var clientSocket = await _listenSocket.AcceptAsync();
                await Console.Out.WriteLineAsync($"Accepted a connection from: {clientSocket.RemoteEndPoint}");

                var proxyNetworkPipe = await ConnectNetworkPipe(clientSocket);
                if (proxyNetworkPipe is not null) {
                    proxyNetworkPipe.Start();
                    _proxyNetworkPipes.Add(proxyNetworkPipe);
                }
            }
        }

        private void PruneNetworkPipes() {
            foreach (var deadStatePipe in _proxyNetworkPipes.Where(i => i.ClosedState)) {
                // _proxyNetworkPipes.Remove(deadStatePipe);
            }
        }

        private async Task<ProxyNetworkPipe?> ConnectNetworkPipe(Socket clientSocket) {
            ProxyNetworkPipe? proxyNetworkPipe = null;

            try {
                Socket? targetSocket = null;
                switch (ManagedProxyEntry.ProxySocketType) {
                    case ProxySocketType.Tcp:
                        targetSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                        break;
                    case ProxySocketType.Udp:
                        targetSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Udp);
                        break;
                    case ProxySocketType.Any:
                        break;
                    default:
                        targetSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                        break;
                }

                if (targetSocket is null) {
                    throw new Exception("Could not connect to target socket.");
                }

                await targetSocket.ConnectAsync(_targetEndpoint);

                proxyNetworkPipe = new ProxyNetworkPipe(clientSocket, targetSocket);
                await Console.Out.WriteLineAsync($"Setup Network Pipe: {proxyNetworkPipe}");
            }
            catch (Exception e) {
                await Console.Out.WriteLineAsync($"Failed to connect to {_targetEndpoint.Address}:{_targetEndpoint.Port}\n{e.Message}");
                clientSocket.Dispose();
            }

            return proxyNetworkPipe;
        }

        public double GetAverageDelayNanoSecond() {
            if (_proxyNetworkPipes.Count <= 0) {
                return 0;
            }
            return _proxyNetworkPipes.Average(i => i.Latency.TotalNanoseconds);
        }

        public void Dispose() {
            _listenSocket.Dispose();

            GC.SuppressFinalize(this);
        }
    }
}
