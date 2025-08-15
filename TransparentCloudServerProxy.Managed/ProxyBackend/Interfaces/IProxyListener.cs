using System.Net.Sockets;

using TransparentCloudServerProxy.Testables.Interfaces;

namespace TransparentCloudServerProxy.ProxyBackend.Interfaces {
    public interface IProxyListener {
        void Dispose();
        void Start(Action<ITestableSocket> acceptedConnection);
        void Stop(CancellationTokenSource sourceToken);
    }
}