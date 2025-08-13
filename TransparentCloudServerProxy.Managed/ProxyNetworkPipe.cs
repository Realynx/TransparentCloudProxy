using System.Net.Sockets;
using System.Threading.Channels;

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
            ForwardTraffic(_clientSocket, _targetSocket);
            ForwardTraffic(_targetSocket, _clientSocket);
        }

        private void ForwardTraffic(Socket source, Socket destination) {
            var channel = Channel.CreateUnbounded<ReadOnlyMemory<byte>>();
            Task.Factory.StartNew(
                () => ReceiveTraffic(source, channel, _cancellationTokenSource.Token),
                _cancellationTokenSource.Token,
                TaskCreationOptions.LongRunning,
                TaskScheduler.Default
            );
            Task.Factory.StartNew(
                () => SendTraffic(destination, channel, _cancellationTokenSource.Token),
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

        private const int BUFFER_SIZE = (int)(65536 * .4);

        private static void ForwardTraffic(Socket source, Socket destination, int threadId, CancellationToken cancellationToken) {
            _delays = new double[50];
            Span<byte> buffer = new byte[BUFFER_SIZE];

            // var stopWatch = new Stopwatch();
            while (!cancellationToken.IsCancellationRequested && source.Connected && destination.Connected) {
                // stopWatch.Restart();

                var bytesRead = source.Receive(buffer, SocketFlags.None);
                destination.Send(buffer[..bytesRead], SocketFlags.None);

                // stopWatch.Stop();
                // _delays[_delayIndex] = stopWatch.Elapsed.TotalMilliseconds;
                // _delayIndex = (_delayIndex + 1) % _delays.Length;
                // if (_delayIndex % 5 == 0) {
                //     var log = $"T{threadId:0}: Min: {_delays.Min():000.000} Max: {_delays.Max():000.000} Avg: {_delays.Average():000.000}  ";
                //     lock (DelayLogLock) {
                //         Console.CursorLeft = log.Length * threadId;
                //         Console.Write(log);
                //     }
                // }
            }
        }

        private static void ReceiveTraffic(Socket source, Channel<ReadOnlyMemory<byte>> channel, CancellationToken cancellationToken) {
            var buffer = new byte[BUFFER_SIZE];

            while (!cancellationToken.IsCancellationRequested && source.Connected) {
                var bytesRead = source.Receive(buffer.AsSpan(), SocketFlags.None);
                var newBuf = buffer.AsSpan(0, bytesRead).ToArray();
                while (!channel.Writer.TryWrite(newBuf)) { }
            }
        }

        private static void SendTraffic(Socket destination, Channel<ReadOnlyMemory<byte>> channel, CancellationToken cancellationToken) {
            while (!cancellationToken.IsCancellationRequested && destination.Connected) {
                if (channel.Reader.TryRead(out var memory)) {
                    destination.Send(memory.Span, SocketFlags.None);
                }
            }
        }
    }
}
