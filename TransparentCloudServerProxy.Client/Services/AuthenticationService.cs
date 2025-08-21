using System.Threading.Tasks;

using TransparentCloudServerProxy.Client.Extentions;
using TransparentCloudServerProxy.Client.Models;
using TransparentCloudServerProxy.Client.Services.Interfaces;

namespace TransparentCloudServerProxy.Client.Services {
    public class AuthenticationService : IAuthenticationService {
        private readonly ILoginStorageService _loginStorageService;
        private readonly IProxyServerService _proxyServerService;

        public AuthenticationService(ILoginStorageService loginStorageService, IProxyServerService proxyServerService) {
            _loginStorageService = loginStorageService;
            _proxyServerService = proxyServerService;
        }

        public bool Login(string server, string credential) {
            var serverUri = server.NormalizeHostUri();
            if (serverUri is null) {
                return false;
            }

            var proxyServer = _proxyServerService.AddServer(new SavedCredential() {
                Credential = credential,
                ReachableAddress = serverUri.ToString()
            });

            if (proxyServer is null) {
                return false;
            }

            return true;
        }

        public async Task LoadAllSavedCredentials() {
            var allSavedLogins = _loginStorageService.GetAllLogins();

            foreach (var savedLogin in allSavedLogins) {
                var proxyServer = await _proxyServerService.AddServer(savedLogin);
                if (proxyServer is null) {
                    continue;
                }
            }
        }
    }
}
