using System.Net;
using System.Net.Sockets;

using Moq;

using TransparentCloudServerProxy.ProxyBackend.Managed;
using TransparentCloudServerProxy.Testables.Interfaces;

namespace TransparentCloudServerProxy.Tests.ManagedProxyTests {
    public class When_Accepting_Client : Using_ManagedProxy {
        protected override void Setup() {
            MockFactories();

            _mockedListener
                .Setup(m => m.Start(It.IsAny<Action<ITestableSocket>>()))
                .Callback<Action<ITestableSocket>>(action => {
                    action(_testableSocket.Object);
                });

            TestableImplementation = new ManagedProxy("Managed", Models.ProxySocketType.Tcp, _listenAddress, _listenPort,
                _targetAddress, _targetPort, _socketFactory.Object, _listenerFactory.Object);
        }

        protected override void Act() {
            TestableImplementation.Start();
            Thread.Sleep(TimeSpan.FromMilliseconds(10));
        }

        [Fact]
        public void Was_ConnectedToAcceptedClient() {
            _socketFactory.Verify(i => i.CreateSocket(It.IsAny<AddressFamily>(), It.IsAny<SocketType>(), It.IsAny<ProtocolType>()));
            _testableSocket.Verify(i => i.ConnectAsync(It.IsAny<IPEndPoint>()));
        }

        [Fact]
        public void Was_ListenrStartedOnlyOnce() {
            _mockedListener.Verify(i => i.Start(It.IsAny<Action<ITestableSocket>>()), Times.Once());
        }

        [Fact]
        public void Was_CorrectSocketType() {
            _listenerFactory.Verify(i => i.CreateProxyListener(It.IsAny<IPEndPoint>(),
                It.Is<Models.ProxySocketType>(i => i == Models.ProxySocketType.Tcp), It.IsAny<CancellationToken>()), Times.Once());
        }

        [Fact]
        public void Was_CorrectListenEndpoint() {
            _listenerFactory.Verify(i => i.CreateProxyListener(It.Is<IPEndPoint>(i => i.Address.ToString() == _listenAddress && i.Port == _listenPort),
                It.IsAny<Models.ProxySocketType>(), It.IsAny<CancellationToken>()), Times.Once());
        }
    }
}
