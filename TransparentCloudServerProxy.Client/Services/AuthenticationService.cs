using System;
using System.Net;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Hosting.Server.Features;

using TransparentCloudServerProxy.Client.Extentions;
using TransparentCloudServerProxy.Client.Services.Interfaces;
using TransparentCloudServerProxy.WebDashboard.SqlDb.Models;

namespace TransparentCloudServerProxy.Client.Services {
    public class AuthenticationService : IAuthenticationService {
        private readonly IUserApi _userApi;
        private readonly ILoginStorageService _loginStorageService;

        private ProxyUser? _currentUser = null;
        private string? _userCredential = null;
        private Uri? _authServer = null;

        public AuthenticationService(IUserApi userApi, ILoginStorageService loginStorageService) {
            _userApi = userApi;
            _loginStorageService = loginStorageService;
        }

        public bool ValidCredentials {
            get {
                return _currentUser is not null;
            }
        }

        public ProxyUser? GetCurrentUser() {
            return _currentUser;
        }

        public async Task<bool> LoginAsync(string server, string credential) {
            var serverUri = server.NormalizeHostUri();
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

                (var serverAddress, var credential) = _loginStorageService.RecoverLogin();
                if (string.IsNullOrWhiteSpace(credential) || serverAddress is null) {
                    return false;
                }

                var recoveredProxyUser = await _userApi.Login(serverAddress, credential);
                if (recoveredProxyUser is null) {
                    return false;
                }

                _authServer = serverAddress;
                _userCredential = credential;
                _currentUser = recoveredProxyUser;
            }

            var proxyUser = await _userApi.Login(_authServer, _userCredential);
            if (proxyUser is null) {
                return false;
            }

            _currentUser = proxyUser;
            return true;
        }

    }
}
