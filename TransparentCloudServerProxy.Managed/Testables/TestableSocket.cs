using System;
using System.Net;
using System.Net.Sockets;

using TransparentCloudServerProxy.Testables.Interfaces;

namespace TransparentCloudServerProxy.Testables {
    public class TestableSocket : Socket, ITestableSocket {
        public TestableSocket(AddressFamily addressFamily, SocketType socketType, ProtocolType protocolType)
            : base(addressFamily, socketType, protocolType) {
        }

        public TestableSocket(Socket acceptedSocket) : base(acceptedSocket.SafeHandle) { }

        public new bool Connected {
            get {
                return base.Connected;

            }
        }

        public new SafeSocketHandle SafeHandle {
            get {
                return base.SafeHandle;

            }
        }

        public new EndPoint? RemoteEndPoint {
            get {
                return base.RemoteEndPoint;
            }
        }

        public new void Bind(EndPoint endPoint) {
            base.Bind(endPoint);
        }

        public new void Listen(int backlog) {
            base.Listen(backlog);
        }

        public new void Close() {
            base.Close();
        }

        public new void Dispose() {
            base.Dispose();
        }

        public new async Task ConnectAsync(IPEndPoint endPoint) {
            await base.ConnectAsync(endPoint);
        }

        public new async Task<TestableSocket> AcceptAsync() {
            var acceptedSocket = await base.AcceptAsync();
            return new TestableSocket(acceptedSocket);
        }

        public new void SetSocketOption(SocketOptionLevel optionLevel, SocketOptionName optionName, bool optionValue) {
            base.SetSocketOption(optionLevel, optionName, optionValue);
        }

        public new int Receive(Span<byte> buffer, SocketFlags socketFlags) {
            return base.Receive(buffer, socketFlags);
        }

        public new int Send(Span<byte> buffer, SocketFlags socketFlags) {
            return base.Send(buffer, socketFlags);
        }
    }
}
