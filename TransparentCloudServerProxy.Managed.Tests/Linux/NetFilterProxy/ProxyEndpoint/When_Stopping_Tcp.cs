using Moq;

using TransparentCloudServerProxy.Managed.Models;

namespace TransparentCloudServerProxy.Managed.Tests.Linux.NetFilterProxy.ProxyEndpoint {
    public class When_Stopping_Tcp : Using_NetFilterProxyEndpoint {
        protected override void Setup() {
            Mocker.Use(new ManagedProxyEntry(_listenAddress, _listenPort, _targetAddress, _targetPort) {
                ProxySocketType = ProxySocketType.Tcp
            });
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
    }
}
