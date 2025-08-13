using System.Diagnostics;
using System.Net.Sockets;
using System.Runtime.CompilerServices;

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
            Task.Factory.StartNew(
                () => ForwardTraffic(_clientSocket, _targetSocket, _cancellationTokenSource.Token),
                _cancellationTokenSource.Token,
                TaskCreationOptions.LongRunning,
                TaskScheduler.Default
            );
            Task.Factory.StartNew(
                () => ForwardTraffic(_targetSocket, _clientSocket, _cancellationTokenSource.Token),
                _cancellationTokenSource.Token,
                TaskCreationOptions.LongRunning,
                TaskScheduler.Default
            );
        }

        private static void ForwardTraffic(Socket source, Socket destination, CancellationToken cancellationToken) {
            Span<byte> buffer = new byte[65536];

            var stopWatch = new Stopwatch();
            while (!cancellationToken.IsCancellationRequested) {
                stopWatch.Restart();

                var bytesRead = source.Receive(buffer, SocketFlags.None);
                destination.Send(buffer[..bytesRead], SocketFlags.None);

                stopWatch.Stop();
                Console.WriteLine($"Delay: {stopWatch.Elapsed.TotalMilliseconds}");
            }
        }
    }
}
