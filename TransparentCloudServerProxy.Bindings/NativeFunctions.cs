using System.Runtime.InteropServices;

namespace TransparentCloudServerProxy.Bindings {
    public class NativeFunctions {
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct ProxyMapping {
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
            public string ListenIp;

            public ushort ListenPort;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
            public string TargetIP;

            public ushort TargetPort;
        }


        [DllImport("TransparentCloudServerProxy.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void StartProxy();
    }
}
