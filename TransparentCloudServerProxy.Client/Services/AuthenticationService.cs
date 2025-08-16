using System;
using System.Net;
using System.Threading.Tasks;

using TransparentCloudServerProxy.Client.Services.Interfaces;
using TransparentCloudServerProxy.WebDashboard.SqlDb.Models;

namespace TransparentCloudServerProxy.Client.Services {
    public class AuthenticationService : IAuthenticationService {
        private readonly IUserApi _userApi;

        private ProxyUser? _currentUser = null;
        private string? _userCredential = null;
        private Uri? _authServer = null;

        public AuthenticationService(IUserApi userApi) {
            _userApi = userApi;
        }

        public bool ValidCredentials {
            get {
                return _currentUser is not null;
            }
        }

        public async Task<bool> LoginAsync(string server, string credential) {
            var serverUri = NormalizeServerUrl(server);
            if (serverUri is null) {
                return false;
            }

            var proxyUser = await _userApi.Login(serverUri, credential);
            if (proxyUser is null) {
                return false;
            }

            _currentUser = proxyUser;
            _userCredential = credential;
            _authServer = serverUri;
            return true;
        }

        public async Task<bool> CheckCredential() {
            if (string.IsNullOrWhiteSpace(_userCredential) || _authServer is null) {
                return false;
            }

            var proxyUser = await _userApi.Login(_authServer, _userCredential);
            if (proxyUser is null) {
                return false;
            }

            _currentUser = proxyUser;
            return true;
        }

        private Uri? NormalizeServerUrl(string input) {
            if (string.IsNullOrWhiteSpace(input)) {
                return null;
            }

            if (!input.StartsWith("http://", StringComparison.OrdinalIgnoreCase) &&
                !input.StartsWith("https://", StringComparison.OrdinalIgnoreCase)) {
                input = "https://" + input;
            }

            if (!Uri.TryCreate(input, UriKind.Absolute, out var uri)) {
                return null;
            }

            var builder = new UriBuilder(uri);
            if (!builder.Path.EndsWith("/")) {
                builder.Path += "/";
            }

            return builder.Uri;
        }

    }
}
