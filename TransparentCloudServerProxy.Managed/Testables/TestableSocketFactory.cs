using System.Net.Sockets;

using TransparentCloudServerProxy.Testables.Interfaces;

namespace TransparentCloudServerProxy.Testables {
    public class TestableSocketFactory : ITestableSocketFactory {
        public ITestableSocket CreateSocket(AddressFamily addressFamily, SocketType socketType, ProtocolType protocolType) {
            return new TestableSocket(addressFamily, socketType, protocolType);
        }
    }
}
