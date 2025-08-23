using Moq;

using TransparentCloudServerProxy.Models;
using TransparentCloudServerProxy.ProxyBackend.WindowsPF;

namespace TransparentCloudServerProxy.Tests.WindowsPfProxyTests {
    public class When_Starting_Tcp : Using_WindowsPFProxy {
        protected override void Setup() {
            MockSystemProgram();

            TestableImplementation = new WindowsPFProxy("WindowsPF", ProxySocketType.Tcp, _listenAddress, _listenPort, _targetAddress, _targetPort);
            TestableImplementation.Netsh = _netshService.Object;
        }

        protected override void Act() {
            TestableImplementation.Start();
        }

        [Fact]
        public void Was_ValidSocketType() {
            _netshService.Verify(i => i.RunCommand(It.Is<string>(i => i.Contains("tcp", StringComparison.OrdinalIgnoreCase))), Times.Once());
        }

        [Fact]
        public void Was_RuleAdded() {
            _netshService.Verify(i => i.RunCommand(It.Is<string>(i => i.StartsWith("interface portproxy add", StringComparison.OrdinalIgnoreCase))), Times.Once());
        }

        [Fact]
        public void Was_EnableTurnOn() {
            Assert.True(TestableImplementation.Enabled);
        }
    }
}
