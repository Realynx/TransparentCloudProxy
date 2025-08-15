using Moq;

using TransparentCloudServerProxy.Managed.Models;
using TransparentCloudServerProxy.ProxyBackend.UnixNetfilter;

namespace TransparentCloudServerProxy.Managed.Tests.NetFilterProxyTests {
    public class When_Starting_Udp : Using_NetFilterProxy {
        protected override void Setup() {
            MockSystemProgram();

            TestableImplementation = new NetFilterProxy(ProxySocketType.Udp, _listenAddress, _listenPort, _targetAddress, _targetPort);
            TestableImplementation.NetFilterProgram = _netFilterService.Object;
        }

        protected override void Act() {
            TestableImplementation.Start();
        }

        [Fact]
        public void Was_ValidSocketType() {
            _netFilterService.Verify(i => i.RunCommand(It.Is<string>(i => i.Contains("udp", StringComparison.OrdinalIgnoreCase))), Times.Once());
        }

        [Fact]
        public void Was_EnableTurnOn() {
            Assert.True(TestableImplementation.Enabled);
        }
    }
}
