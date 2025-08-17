using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;

using TransparentCloudServerProxy.Client.Services.Interfaces;
using TransparentCloudServerProxy.ProxyBackend;

namespace TransparentCloudServerProxy.Client.Services.Api {
    public class ProxyApi : IProxyApi {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IAuthenticationService _authenticationService;

        public ProxyApi(IHttpClientFactory httpClientFactory, IAuthenticationService authenticationService) {
            _httpClientFactory = httpClientFactory;
            _authenticationService = authenticationService;
        }

        public async Task<bool> UpdateOrAddProxy(Proxy proxy) {
            using var httpClient = CreateHttpClient();

            var proxyUserResponse = await httpClient.PostAsJsonAsync("/ProxyApi/AddOrModifyProxy", proxy);
            return proxyUserResponse.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteProxy(Proxy proxy) {
            using var httpClient = CreateHttpClient();

            var proxyUserResponse = await httpClient.PostAsJsonAsync("/ProxyApi/RemoveProxy", proxy);
            return proxyUserResponse.IsSuccessStatusCode;
        }

        private HttpClient CreateHttpClient() {
            var httpClient = _httpClientFactory.CreateClient();
            (var authServer, var credential) = _authenticationService.GetCurrentCredentials();

            httpClient.BaseAddress = authServer;
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Key", credential);
            return httpClient;
        }
    }
}
