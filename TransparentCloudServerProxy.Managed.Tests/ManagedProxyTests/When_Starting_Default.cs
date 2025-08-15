using Moq;

using TransparentCloudServerProxy.ProxyBackend.ManagedProxy;

namespace TransparentCloudServerProxy.Managed.Tests.ManagedProxyTests {
    public class When_Starting_Default : Using_ManagedProxy {
        protected override void Setup() {
            TestableImplementation = new ManagedProxy(Models.ProxySocketType.Any, _listenAddress, _listenPort, _targetAddress, _targetPort);
        }

        protected override void Act() {
            TestableImplementation.Start();
        }

        [Fact]
        public void Was_ValidTargetEndpoint() {

        }
    }
}
