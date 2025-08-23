using TransparentCloudServerProxy.WebDashboard.Services.Interfaces;

namespace TransparentCloudServerProxy.WebDashboard.Services {
    public class PublicAddressService : IPublicAddressService {
        private readonly IHttpClientFactory _httpClientFactory;

        public PublicAddressService(IHttpClientFactory httpClientFactory) {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<string> GetPublicAddress() {
            using var httpClient = _httpClientFactory.CreateClient();

            var addressResponse = await httpClient.GetAsync("https://Icanhazip.com");

            if (!addressResponse.IsSuccessStatusCode) {
                return string.Empty;
            }

            return await addressResponse.Content.ReadAsStringAsync();
        }
    }
}
