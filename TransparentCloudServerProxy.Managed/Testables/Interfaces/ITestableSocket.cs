using System.Net;
using System.Net.Sockets;

namespace TransparentCloudServerProxy.Testables.Interfaces {
    public interface ITestableSocket {
        SafeSocketHandle SafeHandle { get; }
        EndPoint? RemoteEndPoint { get; }
        bool Connected { get; }

        Task<TestableSocket> AcceptAsync();
        void Bind(EndPoint endPoint);
        void Close();
        Task ConnectAsync(IPEndPoint endPoint);
        void Dispose();
        void Listen(int backlog);
        int Receive(Span<byte> buffer, SocketFlags socketFlags);
        int Send(Span<byte> buffer, SocketFlags socketFlags);
        void SetSocketOption(SocketOptionLevel optionLevel, SocketOptionName optionName, bool optionValue);
    }
}