using System.Net.Sockets;
using System.Runtime.InteropServices;

using TransparentCloudServerProxy.Managed.Interfaces;

namespace TransparentCloudServerProxy.Managed.NativeC {
    public partial class NativeCProxyNetworkPipe : IProxyNetworkPipe {
        // Windows: "proxypipe.dll"; Linux: "libproxypipe.so"
        private const string LIB_NAME =
#if WINDOWS
                "TransparentCloudServerProxy.CNative";
#else
                    "TransparentCloudServerProxy.CNative";
#endif

        private nint _handle;
        private readonly Socket _client;
        private readonly Socket _target;

        [LibraryImport(LIB_NAME)]
        [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
        private static partial nint ProxyPipe_Create(nint hClient, nint hTarget);

        [LibraryImport(LIB_NAME)]
        [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
        private static partial void ProxyPipe_Start(nint hPipe);

        [LibraryImport(LIB_NAME)]
        [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
        private static partial void ProxyPipe_Stop(nint hPipe);

        [LibraryImport(LIB_NAME)]
        [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
        private static partial void ProxyPipe_Destroy(nint hPipe);

        [LibraryImport(LIB_NAME)]
        [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
        private static partial ulong ProxyPipe_GetAverageLatencyNs(nint hPipe);

        public NativeCProxyNetworkPipe(Socket client, Socket target) {
            _client = client ?? throw new ArgumentNullException(nameof(client));
            _target = target ?? throw new ArgumentNullException(nameof(target));

            // Ensure sockets stay alive for the duration
            var hClient = _client.SafeHandle.DangerousGetHandle();
            var hTarget = _target.SafeHandle.DangerousGetHandle();

            _handle = ProxyPipe_Create(hClient, hTarget);
            if (_handle == nint.Zero) {
                throw new InvalidOperationException("ProxyPipe_Create failed.");
            }
        }

        public void Start() => ProxyPipe_Start(_handle);

        public void Stop() => ProxyPipe_Stop(_handle);

        public TimeSpan Latency {
            get {
                var ns = ProxyPipe_GetAverageLatencyNs(_handle);
                checked { return TimeSpan.FromTicks((long)(ns / TimeSpan.NanosecondsPerTick)); }
            }
        }

        public override string ToString() {
            return $"{_client.RemoteEndPoint} <-> {_target.RemoteEndPoint}";
        }

        public void Dispose() {
            try { Stop(); }
            catch { }

            if (_handle != nint.Zero) {
                ProxyPipe_Destroy(_handle);
                _handle = nint.Zero;
            }

            _client.Dispose();
            _target.Dispose();

            GC.SuppressFinalize(this);
        }
    }
}
