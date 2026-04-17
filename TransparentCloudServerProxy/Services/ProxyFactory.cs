using TransparentCloudServerProxy.Interfaces;
using TransparentCloudServerProxy.ProxyBackend;
using TransparentCloudServerProxy.ProxyBackend.Interfaces;
using TransparentCloudServerProxy.ProxyBackend.Managed;
using TransparentCloudServerProxy.ProxyBackend.NativeC;
using TransparentCloudServerProxy.ProxyBackend.UnixNetfilter;
using TransparentCloudServerProxy.ProxyBackend.WindowsPF;

namespace TransparentCloudServerProxy.Services {
    public class ProxyFactory : IProxyFactory {
        public IProxy Create(Proxy proxy) {
            ArgumentNullException.ThrowIfNull(proxy);

            return proxy.PacketEngine switch {
                PacketEngine.NetFilter => NetFilterProxy.FromInstance(proxy),
                PacketEngine.NativeC => NativeCProxy.FromInstance(proxy),
                PacketEngine.WindowsPF => WindowsPFProxy.FromInstance(proxy),
                PacketEngine.Managed => ManagedProxy.FromInstance(proxy),
                _ => CreateManagedProxy(proxy)
            };
        }

        private static IProxy CreateManagedProxy(Proxy proxy) {
            proxy.PacketEngine = PacketEngine.Managed;
            return ManagedProxy.FromInstance(proxy);
        }
    }
}
