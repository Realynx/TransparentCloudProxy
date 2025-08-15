using Moq;

using TransparentCloudServerProxy.Managed.Models;

namespace TransparentCloudServerProxy.Managed.Tests.Linux.NetFilterProxy.ProxyEndpoint {
    public class When_Default_ConfigureProxies : Using_NetFilterProxyEndpoint {
        protected override void Setup() {
            Mocker.Use(new ManagedProxyEntry(_listenAddress, _listenPort, _targetAddress, _targetPort));
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
    }
}
