using System.Diagnostics;
using System.Net.Sockets;

namespace TransparentCloudServerProxy.Managed {
    internal class ProxyNetworkPipe : IDisposable {
        private readonly CancellationTokenSource _cancellationTokenSource = new();
        private readonly Socket _clientSocket;
        private readonly Socket _targetSocket;

        public ProxyNetworkPipe(Socket clientSocket, Socket targetSocket) {
            _clientSocket = clientSocket;
            _targetSocket = targetSocket;
        }

        public override string ToString() {
            return $"{_clientSocket.RemoteEndPoint} <-> {_targetSocket.RemoteEndPoint}";
        }

        public void Dispose() {
            Stop();

            _clientSocket.Dispose();
            _targetSocket.Dispose();
        }

        public void Stop() {
            _cancellationTokenSource.Cancel();
        }

        public void ProxyBidirectional() {
            ForwardTraffic(_clientSocket, _targetSocket, _cancellationTokenSource.Token);
            ForwardTraffic(_targetSocket, _clientSocket, _cancellationTokenSource.Token);
        }

        private static async Task ForwardTraffic(Socket source, Socket destination, CancellationToken cancellationToken) {
            var buffer = new byte[65536];

            var stopWatch = new Stopwatch();
            while (!cancellationToken.IsCancellationRequested) {
                stopWatch.Start();

                var memory = buffer.AsMemory();
                var bytesRead = await source.ReceiveAsync(memory, SocketFlags.None, cancellationToken);
                await destination.SendAsync(memory[..bytesRead], SocketFlags.None, cancellationToken);

                stopWatch.Stop();
                Console.WriteLine($"Delay: {stopWatch.Elapsed.TotalMilliseconds}");

                stopWatch.Reset();
            }
        }
    }
}
