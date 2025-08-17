using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;

using TransparentCloudServerProxy.Client.Services.Interfaces;
using TransparentCloudServerProxy.WebDashboard.SqlDb.Models;

namespace TransparentCloudServerProxy.Client.Services.Api {
    internal class UserApi : IUserApi {
        private readonly IHttpClientFactory _httpClientFactory;

        public UserApi(IHttpClientFactory httpClientFactory) {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<ProxyUser?> Login(Uri endpoint, string credential) {
            using var httpClient = _httpClientFactory.CreateClient();
            httpClient.BaseAddress = endpoint;
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Key", credential);

            var proxyUserResponse = await httpClient.GetAsync("/User/Get");
            if (!proxyUserResponse.IsSuccessStatusCode) {
                return null;
            }

            var proxyUser = await proxyUserResponse.Content.ReadFromJsonAsync<ProxyUser>();
            return proxyUser;
        }
    }
}
