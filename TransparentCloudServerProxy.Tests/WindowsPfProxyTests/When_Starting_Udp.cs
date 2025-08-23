using TransparentCloudServerProxy.Managed.Models;
using TransparentCloudServerProxy.ProxyBackend.WindowsPF;

namespace TransparentCloudServerProxy.Managed.Tests.WindowsPfProxyTests {
    public class When_Starting_Udp : Using_WindowsPFProxy {
        protected override void Setup() {
            MockSystemProgram();
        }

        [Fact]
        public void Was_Error() {
            Assert.ThrowsAny<Exception>(() => new WindowsPFProxy("WindowsPF", ProxySocketType.Udp, _listenAddress, _listenPort, _targetAddress, _targetPort));
        }
    }
}
