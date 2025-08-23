using System.Net;
using System.Net.Sockets;

using Moq;

using TestingShared;

using TransparentCloudServerProxy.Models;
using TransparentCloudServerProxy.ProxyBackend;
using TransparentCloudServerProxy.ProxyBackend.Interfaces;
using TransparentCloudServerProxy.Testables.Interfaces;

namespace TransparentCloudServerProxy.Tests.ProxyListenerTests {
    public class Using_ProxyListener : SpecAutoMocker<IProxyListener, ProxyListener> {
        protected CancellationTokenSource _cancellationTokenSource;
        protected Mock<ITestableSocket> _testableSocket;
        protected Mock<ITestableSocketFactory> _socketFactory;
        protected ITestableSocket acceptedSocket;

        protected IPEndPoint _listenEndpoint;
        protected string _listenAddress = "0.0.0.0";
        protected int _listenPort = 25565;
        protected ProxySocketType proxySocketType = ProxySocketType.Any;

        public Using_ProxyListener() {
            _cancellationTokenSource = new();
            _listenEndpoint = IPEndPoint.Parse($"{_listenAddress}:{_listenPort}");
            Init(false);
        }

        protected void MockListenSocket() {
            _testableSocket = Mocker.GetMock<ITestableSocket>();
            _socketFactory = Mocker.GetMock<ITestableSocketFactory>();

            _socketFactory
                .Setup(i => i.CreateSocket(It.IsAny<AddressFamily>(), It.IsAny<SocketType>(), It.IsAny<ProtocolType>()))
                .Returns(_testableSocket.Object);
        }
    }
}
