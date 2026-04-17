using TransparentCloudServerProxy.Interfaces;
using TransparentCloudServerProxy.ProxyBackend;
using TransparentCloudServerProxy.ProxyBackend.Interfaces;
using TransparentCloudServerProxy.SystemTools;

namespace TransparentCloudServerProxy.Services {
    public class PacketFilterResetService : IPacketFilterResetService {
        private readonly Netsh _netsh;
        private readonly NetFilter _netFilter;

        public PacketFilterResetService(Netsh? netsh = null, NetFilter? netFilter = null) {
            _netsh = netsh ?? new Netsh();
            _netFilter = netFilter ?? new NetFilter();
        }

        public void Reset(IEnumerable<IProxy> proxies) {
            var configuredProxies = proxies?.ToArray() ?? [];
            if (configuredProxies.Length == 0) {
                return;
            }

            if (configuredProxies.Any(i => i.PacketEngine == PacketEngine.WindowsPF)) {
                _netsh.ResetState();
            }

            if (configuredProxies.Any(i => i.PacketEngine == PacketEngine.NetFilter)) {
                _netFilter.ResetTables();
            }
        }
    }
}
