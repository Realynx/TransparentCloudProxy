using Moq;

using TransparentCloudServerProxy.ProxyBackend.UnixNetfilter;

namespace TransparentCloudServerProxy.Tests.NetFilterProxyTests {
    public class When_Starting_Default : Using_NetFilterProxy {
        protected override void Setup() {
            MockSystemProgram();

            TestableImplementation = new NetFilterProxy("NetFilter", Models.ProxySocketType.Any, _listenAddress, _listenPort, _targetAddress, _targetPort);
            TestableImplementation.NetFilterProgram = _netFilterService.Object;
        }

        protected override void Act() {
            TestableImplementation.Start();
        }

        [Fact]
        public void Was_ValidTargetEndpoint() {
            _netFilterService.Verify(i => i.RunCommand(It.Is<string>(i => i.Contains($"{_targetAddress}:{_targetPort}"))), Times.Once());
        }

        [Fact]
        public void Was_ValidDestPort() {
            _netFilterService.Verify(i => i.RunCommand(It.Is<string>(i => i.Contains($"dport {_listenPort}"))), Times.Once());
        }

        [Fact]
        public void Was_ValidIpRule() {
            _netFilterService.Verify(i => i.RunCommand(It.Is<string>(i => i.Contains($"add rule ip"))), Times.Once());
        }

        [Fact]
        public void Was_EnableTurnOn() {
            Assert.True(TestableImplementation.Enabled);
        }
    }
}
