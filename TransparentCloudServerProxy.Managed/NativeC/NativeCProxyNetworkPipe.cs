using System.Net.Sockets;
using System.Runtime.InteropServices;

using TransparentCloudServerProxy.Managed.Interfaces;

namespace TransparentCloudServerProxy.Managed.NativeC {
    public class NativeCProxyNetworkPipe : IDisposable, IProxyNetworkPipe {
        // Windows: "proxypipe.dll"; Linux: "libproxypipe.so"
        private const string LIB_NAME =
#if WINDOWS
                "TransparentCloudServerProxy.CNative";
#else
                    "TransparentCloudServerProxy.CNative";
#endif

        private IntPtr _native;
        private readonly Socket _client;
        private readonly Socket _target;

        [DllImport(LIB_NAME, CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr ProxyPipe_Create(IntPtr clientHandle, IntPtr targetHandle);

        [DllImport(LIB_NAME, CallingConvention = CallingConvention.Cdecl)]
        private static extern void ProxyPipe_Start(IntPtr p);

        [DllImport(LIB_NAME, CallingConvention = CallingConvention.Cdecl)]
        private static extern void ProxyPipe_Stop(IntPtr p);

        [DllImport(LIB_NAME, CallingConvention = CallingConvention.Cdecl)]
        private static extern void ProxyPipe_Destroy(IntPtr p);

        [DllImport(LIB_NAME, CallingConvention = CallingConvention.Cdecl)]
        private static extern ulong ProxyPipe_GetAverageLatencyNs(IntPtr p);

        public NativeCProxyNetworkPipe(Socket client, Socket target) {
            _client = client ?? throw new ArgumentNullException(nameof(client));
            _target = target ?? throw new ArgumentNullException(nameof(target));

            // Ensure sockets stay alive for the duration
            var ch = _client.SafeHandle.DangerousGetHandle();
            var th = _target.SafeHandle.DangerousGetHandle();

            _native = ProxyPipe_Create(ch, th);
            if (_native == IntPtr.Zero) {
                throw new InvalidOperationException("ProxyPipe_Create failed.");
            }
        }

        public void Start() => ProxyPipe_Start(_native);

        public void Stop() => ProxyPipe_Stop(_native);

        public TimeSpan Latency {
            get {
                var ns = ProxyPipe_GetAverageLatencyNs(_native);
                checked { return TimeSpan.FromTicks((long)(ns / 100)); } // 1 tick = 100 ns
            }
        }

        public override string ToString() {
            return $"{_client.RemoteEndPoint} <-> {_target.RemoteEndPoint}";
        }

        public void Dispose() {
            try { Stop(); }
            catch { }
            if (_native != IntPtr.Zero) {
                ProxyPipe_Destroy(_native);
                _native = IntPtr.Zero;
            }

            _client.Dispose();
            _target.Dispose();
        }
    }
}
