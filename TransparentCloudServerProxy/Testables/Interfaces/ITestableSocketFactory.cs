using System.Net.Sockets;

namespace TransparentCloudServerProxy.Testables.Interfaces {
    public interface ITestableSocketFactory {
        ITestableSocket CreateSocket(AddressFamily addressFamily, SocketType socketType, ProtocolType protocolType);
    }
}