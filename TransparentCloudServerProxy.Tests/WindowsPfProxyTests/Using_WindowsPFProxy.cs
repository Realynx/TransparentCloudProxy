using Moq;

using TestingShared;

using TransparentCloudServerProxy.Interfaces;
using TransparentCloudServerProxy.ProxyBackend.Interfaces;
using TransparentCloudServerProxy.ProxyBackend.WindowsPF;

namespace TransparentCloudServerProxy.Tests.WindowsPfProxyTests {
    public class Using_WindowsPFProxy : SpecAutoMocker<IProxy, WindowsPFProxy> {
        protected Mock<ISystemProgram> _netshService;

        protected string _listenAddress = "0.0.0.0";
        protected int _listenPort = 443;

        protected string _targetAddress = "10.0.0.1";
        protected int _targetPort = 443;

        public Using_WindowsPFProxy() {
            Init(false);
        }

        protected void MockSystemProgram() {
            _netshService = Mocker.GetMock<ISystemProgram>();

            _netshService
                .Setup(i => i.RunCommand(It.IsAny<string>()))
                .Returns(string.Empty);
        }
    }
}
