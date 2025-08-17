using System.Net;
using System.Net.Sockets;
using System.Threading;

using Moq;

using TransparentCloudServerProxy.Managed.Models;
using TransparentCloudServerProxy.ProxyBackend;

namespace TransparentCloudServerProxy.Managed.Tests.ProxyListenerTests {
    public class When_StopListen_Tcp : Using_ProxyListener {
        protected override void Setup() {
            _cancellationTokenSource = new();
            MockListenSocket();

            TestableImplementation = new ProxyListener(_listenEndpoint, ProxySocketType.Tcp, _cancellationTokenSource.Token, _socketFactory.Object);
        }

        protected override void Act() {
            TestableImplementation.Start(client => acceptedSocket = client);
            Thread.Sleep(TimeSpan.FromMilliseconds(10));
            TestableImplementation.Stop(_cancellationTokenSource);
        }

        [Fact]
        public void Was_SocketTcp() {
            _socketFactory.Verify(i => i.CreateSocket(It.IsAny<AddressFamily>(), It.IsAny<SocketType>(), It.Is<ProtocolType>(i => i == ProtocolType.Tcp)), Times.Once());
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

        [Fact]
        public void Was_SocketClosed() {
            _testableSocket.Verify(i => i.Close(), Times.Once());
        }

        [Fact]
        public void Was_SocketDiposed() {
            _testableSocket.Verify(i => i.Dispose(), Times.Once());
        }
    }
}
