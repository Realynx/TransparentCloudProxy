using System.Diagnostics;
using System.Net.Sockets;
using System.Threading.Channels;

using TransparentCloudServerProxy.Managed.Interfaces;

namespace TransparentCloudServerProxy.Managed.ManagedCode {
    internal class ProxyNetworkPipe : IProxyNetworkPipe {
        private const int BUFFER_SIZE = 4096;

        private readonly CancellationTokenSource _cancellationTokenSource = new();
        private readonly Socket _clientSocket;
        private readonly Socket _targetSocket;
        private readonly long[] _latencies = new long[50];
        private long _latencyIndex;

        public TimeSpan Latency {
            get {
                return TimeSpan.FromTicks((long)_latencies.Average());
            }
        }

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

        public void Start() {
            ForwardTraffic(_clientSocket, _targetSocket);
            ForwardTraffic(_targetSocket, _clientSocket);
        }

        private void ForwardTraffic(Socket source, Socket destination) {
            // var payloadChannel = Channel.CreateBounded<Payload>(16);
            // var bufferChannel = Channel.CreateBounded<byte[]>(16);
            // while (bufferChannel.Writer.TryWrite(new byte[BUFFER_SIZE])) { }
            //
            // Task.Factory.StartNew(
            //     () => ReceiveTraffic(source, bufferChannel, payloadChannel, _cancellationTokenSource.Token),
            //     _cancellationTokenSource.Token,
            //     TaskCreationOptions.LongRunning,
            //     TaskScheduler.Default
            // );
            // Task.Factory.StartNew(
            //     () => SendTraffic(destination, bufferChannel, payloadChannel, _cancellationTokenSource.Token),
            //     _cancellationTokenSource.Token,
            //     TaskCreationOptions.LongRunning,
            //     TaskScheduler.Default
            // );

            Task.Factory.StartNew(
                () => ForwardTraffic(source, destination, _cancellationTokenSource.Token),
                _cancellationTokenSource.Token,
                TaskCreationOptions.LongRunning,
                TaskScheduler.Default
            );
        }

        private void ForwardTraffic(Socket source, Socket destination, CancellationToken cancellationToken) {
            cancellationToken.UnsafeRegister(static state => {
                var socket = (Socket)state!;
                socket.Disconnect(false);
            }, source);

            Span<byte> buffer = new byte[BUFFER_SIZE];

            var sw = new Stopwatch();
            while (!cancellationToken.IsCancellationRequested && source.Connected && destination.Connected) {
                sw.Restart();

                var bytesRead = source.Receive(buffer, SocketFlags.None);

                var bytesSent = 0;
                while (bytesSent < bytesRead) {
                    bytesSent += destination.Send(buffer[bytesSent..bytesRead], SocketFlags.None);
                }

                sw.Stop();
                var idx = Interlocked.Increment(ref _latencyIndex);
                _latencies[idx % _latencies.Length] = sw.ElapsedTicks;
            }
        }

        private static void ReceiveTraffic(Socket source, Channel<byte[]> bufferChannel, Channel<Payload> payloadChannel, CancellationToken cancellationToken) {
            while (!cancellationToken.IsCancellationRequested && source.Connected) {
                byte[]? buffer;
                while (!bufferChannel.Reader.TryRead(out buffer)) { }

                var timestamp = Stopwatch.GetTimestamp();
                var bytesRead = source.Receive(buffer.AsSpan(), SocketFlags.None);

                while (!payloadChannel.Writer.TryWrite(new Payload(buffer, bytesRead, timestamp))) { }
            }
        }

        private void SendTraffic(Socket destination, Channel<byte[]> bufferChannel, Channel<Payload> payloadChannel, CancellationToken cancellationToken) {
            while (!cancellationToken.IsCancellationRequested && destination.Connected) {
                if (payloadChannel.Reader.TryRead(out var payload)) {
                    var bytesSent = 0;
                    while (bytesSent < payload.Length) {
                        bytesSent = destination.Send(payload.Buffer.AsSpan()[bytesSent..payload.Length], SocketFlags.None);
                    }

                    while (!bufferChannel.Writer.TryWrite(payload.Buffer)) { }

                    var latency = Stopwatch.GetTimestamp() - payload.Timestamp;
                    var idx = Interlocked.Increment(ref _latencyIndex);
                    _latencies[idx % _latencies.Length] = (long)((double)TimeSpan.TicksPerSecond / Stopwatch.Frequency * latency);
                }
            }
        }

        private readonly record struct Payload(byte[] Buffer, int Length, long Timestamp);
    }
}
