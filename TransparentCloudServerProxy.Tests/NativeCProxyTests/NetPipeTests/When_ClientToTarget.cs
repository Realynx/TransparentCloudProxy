using TransparentCloudServerProxy.ProxyBackend.Managed;

namespace TransparentCloudServerProxy.Tests.NativeCProxyTests.NetPipeTests {
    public class When_ClientToTarget : Using_ProxyNetworkPipe {
        protected override void Setup() {
            MockSockets();
            TestableImplementation = new ProxyNetworkPipe(_testClientSocket.Object, _testTargetSocket.Object);
        }

        protected override void Act() {
            TestableImplementation.Start();
        }

    }
}
