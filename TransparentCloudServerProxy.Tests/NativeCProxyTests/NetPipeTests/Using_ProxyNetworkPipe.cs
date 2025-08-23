using Moq;

using TestingShared;

using TransparentCloudServerProxy.Managed.Interfaces;
using TransparentCloudServerProxy.ProxyBackend.ManagedCode;
using TransparentCloudServerProxy.Testables.Interfaces;

namespace TransparentCloudServerProxy.Managed.Tests.NativeCProxyTests.NetPipeTests {
    public class Using_ProxyNetworkPipe : SpecAutoMocker<IProxyNetworkPipe, ProxyNetworkPipe> {
        protected Mock<ITestableSocket> _testClientSocket;
        protected Mock<ITestableSocket> _testTargetSocket;

        public Using_ProxyNetworkPipe() {
            Init(false);
        }

        protected void MockSockets() {
            _testClientSocket = Mocker.GetMock<ITestableSocket>();
            _testTargetSocket = Mocker.GetMock<ITestableSocket>();
        }
    }
}
