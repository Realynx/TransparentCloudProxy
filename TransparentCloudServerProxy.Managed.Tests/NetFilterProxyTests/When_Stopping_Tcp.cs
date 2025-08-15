using Moq;

using TransparentCloudServerProxy.Managed.Models;
using TransparentCloudServerProxy.ProxyBackend.UnixNetfilter;

namespace TransparentCloudServerProxy.Managed.Tests.NetFilterProxyTests {
    public class When_Stopping_Tcp : Using_NetFilterProxy {
        protected override void Setup() {
            MockSystemProgram();

            TestableImplementation = new NetFilterProxy(ProxySocketType.Tcp, _listenAddress, _listenPort, _targetAddress, _targetPort);
            TestableImplementation.NetFilterProgram = _netFilterService.Object;
        }

        protected override void Act() {
            TestableImplementation.Start();
            TestableImplementation.Stop();
        }

        [Fact]
        public void Was_DeleteCalled() {
            _netFilterService.Verify(i => i.RunCommand(It.Is<string>(i =>
                i.StartsWith("delete rule ip", StringComparison.OrdinalIgnoreCase) && i.Contains("tcp", StringComparison.OrdinalIgnoreCase)))
            , Times.Once());
        }

        [Fact]
        public void Was_NotUdp() {
            _netFilterService.Verify(i => i.RunCommand(It.Is<string>(i => i.Contains("udp", StringComparison.OrdinalIgnoreCase))), Times.Never());
        }

        [Fact]
        public void Was_EnableTurnOff() {
            Assert.False(TestableImplementation.Enabled);
        }
    }
}
