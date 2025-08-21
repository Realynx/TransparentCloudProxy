using TransparentCloudServerProxy.Client.Models;
using TransparentCloudServerProxy.Client.Services.Api;

namespace TransparentCloudServerProxy.Client.Services.Interfaces {
    public interface IProxyServerFactory {
        ProxyServer CreateProxyServer(SavedCredential savedCredential);
    }
}