using System.Net;
using System.Net.Sockets;

using TransparentCloudServerProxy.Managed.Interfaces;
using TransparentCloudServerProxy.Managed.Models;

namespace TransparentCloudServerProxy.Managed.NativeC {
    public class NativeCProxyEndpoint : IDisposable, IProxyEndpoint {
        public ManagedProxyEntry ManagedProxyEntry { get; }

        private readonly IPEndPoint _targetEndpoint;
        private readonly List<NativeCProxyNetworkPipe> _proxyNetworkPipes = new();

        private Socket _listenSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        private CancellationTokenSource _cancellationTokenSource = new();

        public NativeCProxyEndpoint(ManagedProxyEntry managedProxyEntry) {
            ManagedProxyEntry = managedProxyEntry;

            _targetEndpoint = new IPEndPoint(IPAddress.Parse(ManagedProxyEntry.TargetAddress), ManagedProxyEntry.TargetPort);
        }

        public override string ToString() {
            return ManagedProxyEntry.ToString();
        }

        public double GetAverageDelayNanoSecond() {
            if (_proxyNetworkPipes.Count <= 0) {
                return 0;
            }

            return _proxyNetworkPipes.Average(i => i.Latency.Nanoseconds);
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
            _listenSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            var listenEndpoint = new IPEndPoint(IPAddress.Parse(ManagedProxyEntry.ListenAddress), ManagedProxyEntry.ListenPort);

            _listenSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            _listenSocket.Bind(listenEndpoint);
            _listenSocket.Listen(128);

            Console.WriteLine($"Listening on: {listenEndpoint}");
        }

        private async Task Listen() {
            while (!_cancellationTokenSource.IsCancellationRequested) {
                var clientSocket = await _listenSocket.AcceptAsync();
                await Console.Out.WriteLineAsync($"Accepted a connection from: {clientSocket.RemoteEndPoint}");

                var proxyNetworkPipe = await ConnectNetworkPipe(clientSocket);
                if (proxyNetworkPipe is not null) {
                    proxyNetworkPipe.Start();
                    _proxyNetworkPipes.Add(proxyNetworkPipe);
                }
            }
        }

        private async Task<NativeCProxyNetworkPipe?> ConnectNetworkPipe(Socket clientSocket) {
            NativeCProxyNetworkPipe? proxyNetworkPipe = null;

            try {
                var targetSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                await targetSocket.ConnectAsync(_targetEndpoint);

                proxyNetworkPipe = new NativeCProxyNetworkPipe(clientSocket, targetSocket);
                await Console.Out.WriteLineAsync($"Setup Network Pipe: {proxyNetworkPipe}");
            }
            catch (Exception e) {
                await Console.Out.WriteLineAsync($"Failed to connect to {_targetEndpoint.Address}:{_targetEndpoint.Port}\n{e.Message}");
                clientSocket.Dispose();
            }

            return proxyNetworkPipe;
        }

        public void Dispose() {
            _listenSocket.Dispose();

            GC.SuppressFinalize(this);
        }
    }
}
