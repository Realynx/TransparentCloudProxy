using System.Net.Sockets;

using TransparentCloudServerProxy.Testables.Interfaces;

namespace TransparentCloudServerProxy.Testables {
    public interface ITestableSocketFactory {
        ITestableSocket CreateSocket(AddressFamily addressFamily, SocketType socketType, ProtocolType protocolType);
    }
}