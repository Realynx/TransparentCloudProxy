using System.Net;

using TransparentCloudServerProxy.Managed.Models;

namespace TransparentCloudServerProxy.ProxyBackend.Interfaces {
    public interface IProxyListenerFactory {
        IProxyListener CreateProxyListener(IPEndPoint listenEndpoint, ProxySocketType proxySocketType, CancellationToken cancellationToken);
    }
}