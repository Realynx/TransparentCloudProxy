using Moq;

using TestingShared;

using TransparentCloudServerProxy.Interfaces;
using TransparentCloudServerProxy.ProxyBackend.Interfaces;
using TransparentCloudServerProxy.ProxyBackend.UnixNetfilter;

namespace TransparentCloudServerProxy.Tests.NetFilterProxyTests {
    public class Using_NetFilterProxy : SpecAutoMocker<IProxy, NetFilterProxy> {
        protected Mock<ISystemProgram> _netFilterService;

        protected string _listenAddress = "0.0.0.0";
        protected int _listenPort = 25565;

        protected string _targetAddress = "0.0.0.0";
        protected int _targetPort = 25565;

        public Using_NetFilterProxy() {
            Init(false);
        }

        protected void MockSystemProgram() {
            _netFilterService = Mocker.GetMock<ISystemProgram>();
            _netFilterService
                .Setup(i => i.RunCommand(It.IsAny<string>()))
                .Returns(string.Empty);
        }
    }
}
