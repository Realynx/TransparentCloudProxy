using System.Net;

using Moq;

using TransparentCloudServerProxy.ProxyBackend.Managed;
using TransparentCloudServerProxy.Testables.Interfaces;

namespace TransparentCloudServerProxy.Managed.Tests.ManagedProxyTests {
    public class When_Starting_Udp : Using_ManagedProxy {
        protected override void Setup() {
            MockFactories();
            TestableImplementation = new ManagedProxy("Managed", Models.ProxySocketType.Udp, _listenAddress, _listenPort,
                _targetAddress, _targetPort, _socketFactory.Object, _listenerFactory.Object);
        }

        protected override void Act() {
            TestableImplementation.Start();
        }

        [Fact]
        public void Was_Enabled() {
            Assert.True(TestableImplementation.Enabled);
        }

        [Fact]
        public void Was_ListenrStarted() {
            _mockedListener.Verify(i => i.Start(It.IsAny<Action<ITestableSocket>>()), Times.Once());
        }

        [Fact]
        public void Was_CorrectSocketType() {
            _listenerFactory.Verify(i => i.CreateProxyListener(It.IsAny<IPEndPoint>(),
                It.Is<Models.ProxySocketType>(i => i == Models.ProxySocketType.Udp), It.IsAny<CancellationToken>()), Times.Once());
        }

        [Fact]
        public void Was_CorrectListenEndpoint() {
            _listenerFactory.Verify(i => i.CreateProxyListener(It.Is<IPEndPoint>(i => i.Address.ToString() == _listenAddress && i.Port == _listenPort),
                It.IsAny<Models.ProxySocketType>(), It.IsAny<CancellationToken>()), Times.Once());
        }
    }
}
