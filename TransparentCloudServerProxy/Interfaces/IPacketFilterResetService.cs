using TransparentCloudServerProxy.ProxyBackend.Interfaces;

namespace TransparentCloudServerProxy.Interfaces {
    public interface IPacketFilterResetService {
        void Reset(IEnumerable<IProxy> proxies);
    }
}
