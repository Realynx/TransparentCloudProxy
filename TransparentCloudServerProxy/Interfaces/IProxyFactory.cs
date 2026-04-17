using TransparentCloudServerProxy.ProxyBackend;
using TransparentCloudServerProxy.ProxyBackend.Interfaces;

namespace TransparentCloudServerProxy.Interfaces {
    public interface IProxyFactory {
        IProxy Create(Proxy proxy);
    }
}
