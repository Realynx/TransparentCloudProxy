using Moq;

using TestingShared;

using TransparentCloudServerProxy.Managed.Interfaces;
using TransparentCloudServerProxy.Managed.Models;
using TransparentCloudServerProxy.Managed.UnixNetfilter;
using TransparentCloudServerProxy.Managed.UnixNetfilter.IpTablesApi;

namespace TransparentCloudServerProxy.Managed.Tests.Linux.NetFilterProxy.ProxyEndpoint {
    public class Using_NetFilterProxyEndpoint : SpecAutoMocker<IProxyEndpoint, NetFilterProxyEndpoint> {
        protected Mock<ISystemProgram> _netFilterService;


        protected string _listenAddress = "0.0.0.0";
        protected int _listenPort = 25565;

        protected string _targetAddress = "0.0.0.0";
        protected int _targetPort = 25565;

        public Using_NetFilterProxyEndpoint() {
            Init();

            // configure defaults for each test that is Using_NetFilterProxyEndpoint.
            _netFilterService = Mocker.GetMock<ISystemProgram>();
            _netFilterService
                .Setup(i => i.RunCommand(It.IsAny<string>()))
                .Returns(string.Empty);

        }
    }
}
