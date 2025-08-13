using System.Collections.Concurrent;
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

        private static readonly double[] Delays = new double[50];
        private static int _delayIndex = 0;

        private static void ForwardTraffic(Socket source, Socket destination, CancellationToken cancellationToken) {
            Span<byte> buffer = new byte[(int)(65536 * .4)];

            // var stopWatch = new Stopwatch();

            while (!cancellationToken.IsCancellationRequested && source.Connected && destination.Connected) {
                // stopWatch.Restart();

                var bytesRead = source.Receive(buffer, SocketFlags.None);
                destination.Send(buffer[..bytesRead], SocketFlags.None);

                // stopWatch.Stop();

                // Delays[_delayIndex] = stopWatch.Elapsed.TotalMilliseconds;
                // _delayIndex = (_delayIndex + 1) % Delays.Length;
                // if (_delayIndex % 5 == 0) {
                //     Console.Write($"\rMin: {Delays.Min():N5} Max: {Delays.Max():N5} Avg: {Delays.Average():N5}");
                // }
            }
        }
    }
}
