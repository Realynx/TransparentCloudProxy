using Moq;

using TransparentCloudServerProxy.Managed.Interfaces;
using TransparentCloudServerProxy.ProxyBackend.UnixNetfilter;

namespace TransparentCloudServerProxy.Managed.Tests.NetFilterProxyTests {
    public class When_Stopping_Default : Using_NetFilterProxy {
        protected override void Setup() {
            MockSystemProgram();

            TestableImplementation = new NetFilterProxy("NetFilter", Models.ProxySocketType.Any, _listenAddress, _listenPort, _targetAddress, _targetPort);
            TestableImplementation.NetFilterProgram = _netFilterService.Object;
        }

        protected override void Act() {
            TestableImplementation.Start();
            TestableImplementation.Stop();
        }

        [Fact]
        public void Was_DeleteCalled() {
            _netFilterService.Verify(i => i.RunCommand(It.Is<string>(i => i.StartsWith("delete rule ip", StringComparison.OrdinalIgnoreCase))), Times.Once());
        }

        [Fact]
        public void Was_EnableTurnOff() {
            Assert.False(TestableImplementation.Enabled);
        }
    }
}
