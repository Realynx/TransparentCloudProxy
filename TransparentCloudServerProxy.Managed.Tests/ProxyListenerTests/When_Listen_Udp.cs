using System.Net;
using System.Net.Sockets;

using Moq;

using TransparentCloudServerProxy.Managed.Models;
using TransparentCloudServerProxy.ProxyBackend;

namespace TransparentCloudServerProxy.Managed.Tests.ProxyListenerTests {
    public class When_Listen_Udp : Using_ProxyListener {
        protected override void Setup() {
            MockListenSocket();

            TestableImplementation = new ProxyListener(_listenEndpoint, ProxySocketType.Udp, _cancellationTokenSource.Token, _socketFactory.Object);
        }

        protected override void Act() {
            TestableImplementation.Start(client => acceptedSocket = client);
            Thread.Sleep(10);
        }

        [Fact]
        public void Was_SocketUdp() {
            _socketFactory.Verify(i => i.CreateSocket(It.IsAny<AddressFamily>(), It.IsAny<SocketType>(), It.Is<ProtocolType>(i => i == ProtocolType.Udp)), Times.Once());
        }

        [Fact]
        public void Was_SocketBound() {
            _testableSocket.Verify(i => i.Bind(It.IsAny<IPEndPoint>()), Times.Once());
        }

        [Fact]
        public void Was_SocketBoundCorrect() {
            _testableSocket.Verify(i => i.Bind(It.Is<IPEndPoint>(i => i.ToString() == _listenEndpoint.ToString())), Times.Once());
        }

        [Fact]
        public void Was_AcceptingClients() {
            _testableSocket.Verify(i => i.AcceptAsync(), Times.AtLeastOnce());
        }

        [Fact]
        public void Was_OptionsConfigured() {
            _testableSocket.Verify(i => i.SetSocketOption(It.IsAny<SocketOptionLevel>(), It.IsAny<SocketOptionName>(), It.IsAny<bool>()), Times.Once());
        }
    }
}
