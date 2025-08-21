using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

using TransparentCloudServerProxy.Client.Models;
using TransparentCloudServerProxy.ProxyBackend;
using TransparentCloudServerProxy.WebDashboard.SqlDb.Models;

namespace TransparentCloudServerProxy.Client.Services.Api {
    public class ProxyServer {
        private readonly SavedCredential _savedCredential;
        private readonly HttpClient _httpClient;

        public ProxyUser ServerUser { get; set; }

        public string Address {
            get {
                return _savedCredential.ReachableAddress;
            }
        }

        public ProxyServer(SavedCredential savedCredential, HttpClient httpClient) {
            _savedCredential = savedCredential;
            _httpClient = httpClient;
        }

        public async Task<bool> UpdateOrAddProxy(Proxy proxy) {
            var proxyUserResponse = await _httpClient.PostAsJsonAsync("/ProxyApi/AddOrModifyProxy", proxy);
            return proxyUserResponse.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteProxy(Proxy proxy) {
            var proxyUserResponse = await _httpClient.PostAsJsonAsync("/ProxyApi/RemoveProxy", proxy);
            return proxyUserResponse.IsSuccessStatusCode;
        }

        public async Task<ProxyUser?> GetUser() {
            var proxyUserResponse = await _httpClient.GetAsync("/User/Get");
            if (!proxyUserResponse.IsSuccessStatusCode) {
                return null;
            }

            var responseUser = await proxyUserResponse.Content.ReadFromJsonAsync<ProxyUser>();
            if (responseUser is null) {
                return null;
            }

            ServerUser = responseUser;
            return responseUser;
        }
    }
}
