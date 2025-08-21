using System.Net.Http;
using System.Net.Http.Headers;

using TransparentCloudServerProxy.Client.Extentions;
using TransparentCloudServerProxy.Client.Models;
using TransparentCloudServerProxy.Client.Services.Interfaces;

namespace TransparentCloudServerProxy.Client.Services.Api {
    public class ProxyServerFactory : IProxyServerFactory {
        private readonly IHttpClientFactory _clientFactory;

        public ProxyServerFactory(IHttpClientFactory clientFactory) {
            _clientFactory = clientFactory;
        }

        public ProxyServer CreateProxyServer(SavedCredential savedCredential) {
            var httpClient = _clientFactory.CreateClient(nameof(IProxyServerFactory));
            httpClient.BaseAddress = savedCredential.ReachableAddress.NormalizeHostUri();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Key", savedCredential.Credential);

            return new ProxyServer(savedCredential, httpClient);
        }
    }
}
