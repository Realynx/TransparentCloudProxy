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
                () => ForwardTraffic(_clientSocket, _targetSocket, 0, _cancellationTokenSource.Token),
                _cancellationTokenSource.Token,
                TaskCreationOptions.LongRunning,
                TaskScheduler.Default
            );
            Task.Factory.StartNew(
                () => ForwardTraffic(_targetSocket, _clientSocket, 1, _cancellationTokenSource.Token),
                _cancellationTokenSource.Token,
                TaskCreationOptions.LongRunning,
                TaskScheduler.Default
            );
        }

        [ThreadStatic]
        private static double[]? _delays;
        [ThreadStatic]
        private static int _delayIndex;

        private static readonly Lock DelayLogLock = new();

        private static void ForwardTraffic(Socket source, Socket destination, int threadId, CancellationToken cancellationToken) {
            _delays = new double[50];

            Span<byte> buffer = new byte[(int)(65536 * .4)];

            var stopWatch = new Stopwatch();

            while (!cancellationToken.IsCancellationRequested && source.Connected && destination.Connected) {
                stopWatch.Restart();

                var bytesRead = source.Receive(buffer, SocketFlags.None);
                destination.Send(buffer[..bytesRead], SocketFlags.None);

                stopWatch.Stop();

                _delays[_delayIndex] = stopWatch.Elapsed.TotalMilliseconds;
                _delayIndex = (_delayIndex + 1) % _delays.Length;
                if (_delayIndex % 5 == 0)
                {
                    var min = _delays.Min();
                    var max = _delays.Max();
                    var avg = _delays.Average();
                    lock (DelayLogLock)
                    {
                        var log = $"T{threadId:0}: Min: {min:000.000} Max: {max:000.000} Avg: {avg:000.000}  ";
                        Console.CursorLeft = log.Length * threadId;
                        Console.Write(log);
                    }
                }
            }
        }
    }
}
