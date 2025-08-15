using Moq;

using TestingShared;

using TransparentCloudServerProxy.ProxyBackend.Interfaces;
using TransparentCloudServerProxy.ProxyBackend.NativeC;
using TransparentCloudServerProxy.Testables.Interfaces;
using TransparentCloudServerProxy.Testables;
using System.Net.Sockets;
using System.Net;

namespace TransparentCloudServerProxy.Managed.Tests.NativeCProxyTests {
    public class Using_NativeCProxy : SpecAutoMocker<IProxy, NativeCProxy> {
        protected Mock<ITestableSocket> _testableSocket;
        protected Mock<IProxyListener> _mockedListener;
        protected Mock<ITestableSocketFactory> _socketFactory;
        protected Mock<IProxyListenerFactory> _listenerFactory;

        protected string _listenAddress = "0.0.0.0";
        protected int _listenPort = 25565;

        protected string _targetAddress = "10.0.1.20";
        protected int _targetPort = 25565;

        public Using_NativeCProxy() {
            Init(false);
        }

        protected void MockFactories() {
            _mockedListener = Mocker.GetMock<IProxyListener>();
            _testableSocket = Mocker.GetMock<ITestableSocket>();
            _socketFactory = Mocker.GetMock<ITestableSocketFactory>();
            _listenerFactory = Mocker.GetMock<IProxyListenerFactory>();

            _socketFactory
                .Setup(i => i.CreateSocket(It.IsAny<AddressFamily>(), It.IsAny<SocketType>(), It.IsAny<ProtocolType>()))
                .Returns(_testableSocket.Object);

            _listenerFactory
                .Setup(i => i.CreateProxyListener(It.IsAny<IPEndPoint>(), It.IsAny<Models.ProxySocketType>(), It.IsAny<CancellationToken>()))
                .Returns(_mockedListener.Object);
        }
    }
}
