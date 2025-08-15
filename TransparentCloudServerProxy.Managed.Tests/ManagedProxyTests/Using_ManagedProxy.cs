using TestingShared;

using TransparentCloudServerProxy.Managed.Interfaces;
using TransparentCloudServerProxy.ProxyBackend;
using TransparentCloudServerProxy.ProxyBackend.ManagedProxy;

namespace TransparentCloudServerProxy.Managed.Tests.ManagedProxyTests {
    public class Using_ManagedProxy : SpecAutoMocker<IProxy, ManagedProxy> {
        protected string _listenAddress = "0.0.0.0";
        protected int _listenPort = 25565;

        protected string _targetAddress = "0.0.0.0";
        protected int _targetPort = 25565;

        public Using_ManagedProxy() {
            Init(false);
        }
    }
}
