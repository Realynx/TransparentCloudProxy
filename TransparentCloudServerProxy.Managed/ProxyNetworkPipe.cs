using System.Buffers;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net.Sockets;
using System.Threading.Channels;

namespace TransparentCloudServerProxy.Managed {
    internal class ProxyNetworkPipe : IDisposable {
        private readonly CancellationTokenSource _cancellationTokenSource = new();
        private readonly Socket _clientSocket;
        private readonly Socket _targetSocket;
        private Timer? _latencyTimer;

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
            _latencyTimer?.Dispose();
        }

        public void Stop() {
            _cancellationTokenSource.Cancel();
        }

        public void ProxyBidirectional() {
            var timestampQueue = new ConcurrentQueue<long>();
            ForwardTraffic(_clientSocket, _targetSocket, timestampQueue);
            ForwardTraffic(_targetSocket, _clientSocket, timestampQueue);

            var delays = new double[50];
            var delayIndex = 0;

            _latencyTimer?.Dispose();
            _latencyTimer = new Timer(_ => {
                while (timestampQueue.TryDequeue(out var timestamp)) {
                    delays[delayIndex] = new TimeSpan((long)((double)TimeSpan.TicksPerSecond / Stopwatch.Frequency * timestamp)).TotalMilliseconds;
                    delayIndex = (delayIndex + 1) % delays.Length;
                }

                var log = $"\r Min: {delays.Min():000.000} Max: {delays.Max():000.000} Avg: {delays.Average():000.000}";
                Console.Write(log);
            }, null, TimeSpan.FromMilliseconds(100), TimeSpan.FromMilliseconds(100));
        }

        private void ForwardTraffic(Socket source, Socket destination, ConcurrentQueue<long> timestampQueue) {
            var channel = Channel.CreateBounded<Payload>(16);
            Task.Factory.StartNew(
                () => ReceiveTraffic(source, channel, _cancellationTokenSource.Token),
                _cancellationTokenSource.Token,
                TaskCreationOptions.LongRunning,
                TaskScheduler.Default
            );
            Task.Factory.StartNew(
                () => SendTraffic(destination, channel, timestampQueue, _cancellationTokenSource.Token),
                _cancellationTokenSource.Token,
                TaskCreationOptions.LongRunning,
                TaskScheduler.Default
            );
        }

        // [ThreadStatic]
        // private static double[]? _delays;
        // [ThreadStatic]
        // private static int _delayIndex;
        //
        // private static readonly Lock DelayLogLock = new();

        private const int BUFFER_SIZE = (int)(65536 * .4);

        private static void ForwardTraffic(Socket source, Socket destination, int threadId, CancellationToken cancellationToken) {
            // _delays = new double[50];
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

        private static void ReceiveTraffic(Socket source, Channel<Payload> channel, CancellationToken cancellationToken) {
            var buffer = new byte[BUFFER_SIZE];

            while (!cancellationToken.IsCancellationRequested && source.Connected) {
                var timestamp = Stopwatch.GetTimestamp();
                var bytesRead = source.Receive(buffer.AsSpan(), SocketFlags.None);

                var newBuff = ArrayPool<byte>.Shared.Rent(bytesRead);
                buffer.AsSpan(0, bytesRead).CopyTo(newBuff);
                while (!channel.Writer.TryWrite(new Payload(newBuff, bytesRead, timestamp))) { }
            }
        }

        private static void SendTraffic(Socket destination, Channel<Payload> channel, ConcurrentQueue<long> timestampQueue, CancellationToken cancellationToken) {
            while (!cancellationToken.IsCancellationRequested && destination.Connected) {
                if (channel.Reader.TryRead(out var payload)) {

                    var bytesSent = 0;
                    while (bytesSent < payload.Length) {
                        bytesSent = destination.Send(payload.Buffer.AsSpan()[bytesSent..payload.Length], SocketFlags.None);
                    }

                    ArrayPool<byte>.Shared.Return(payload.Buffer);

                    // Profiling
                    var latency = Stopwatch.GetTimestamp() - payload.Timestamp;
                    timestampQueue.Enqueue(latency);
                }
            }
        }

        private readonly record struct Payload(byte[] Buffer, int Length, long Timestamp);
    }
}
