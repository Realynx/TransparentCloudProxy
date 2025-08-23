using System.Net;

using TransparentCloudServerProxy.Models;
using TransparentCloudServerProxy.ProxyBackend.Interfaces;

namespace TransparentCloudServerProxy.ProxyBackend {
    public class ProxyListenerFactory : IProxyListenerFactory {
        public IProxyListener CreateProxyListener(IPEndPoint listenEndpoint, ProxySocketType proxySocketType, CancellationToken cancellationToken) {
            return new ProxyListener(listenEndpoint, proxySocketType, cancellationToken);
        }
    }
}
