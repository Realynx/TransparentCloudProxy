using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;

using TransparentCloudServerProxy.Client.Services.Interfaces;
using TransparentCloudServerProxy.ProxyBackend;
using TransparentCloudServerProxy.WebDashboard.SqlDb.Models;

namespace TransparentCloudServerProxy.Client.Services.Api {
    public class ProxyApi {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IAuthenticationService _authenticationService;

        public ProxyApi(IHttpClientFactory httpClientFactory, IAuthenticationService authenticationService) {
            _httpClientFactory = httpClientFactory;
            _authenticationService = authenticationService;
        }

        public async Task<bool> StartProxy(Proxy proxy) {
            using var httpClient = CreateHttpClient();

            var proxyUserResponse = await httpClient.GetAsync("/ProxyApi/StartProxy");
            if (!proxyUserResponse.IsSuccessStatusCode) {
                return false;
            }

            var proxyUser = await proxyUserResponse.Content.ReadFromJsonAsync<Proxy>();
            return true;
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
