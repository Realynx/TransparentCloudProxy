using Moq;

using TransparentCloudServerProxy.Managed.Models;

namespace TransparentCloudServerProxy.Managed.Tests.Linux.NetFilterProxy.ProxyEndpoint {
    public class When_Udp_ConfigureProxies : Using_NetFilterProxyEndpoint {

        protected override void Setup() {
            Mocker.Use(new ManagedProxyEntry(_listenAddress, _listenPort, _targetAddress, _targetPort) {
                ProxySocketType = ProxySocketType.Udp
            });
        }

        protected override void Act() {
            TestableImplementation.Start();
        }

        [Fact]
        public void Was_ValidSocketType() {
            _netFilterService.Verify(i => i.RunCommand(It.Is<string>(i => i.Contains("udp", StringComparison.OrdinalIgnoreCase))), Times.Once());
        }
    }
}
