using System;
using System.Collections.Generic;
using System.Linq;

using TransparentCloudServerProxy.Client.Extentions;
using TransparentCloudServerProxy.Client.Models;
using TransparentCloudServerProxy.Client.Services.Interfaces;

namespace TransparentCloudServerProxy.Client.Services {
    public class LoginStorageService : ILoginStorageService {
        private readonly ISecureFileStorageService _secureFileStorageService;

        public LoginStorageService(ISecureFileStorageService secureFileStorageService) {
            _secureFileStorageService = secureFileStorageService;
        }

        public void StoreLogin(string credential, Uri serverAddress) {
            var storedServerCredentuals = _secureFileStorageService.GetModel<StoredServerCredentials>();
            if (storedServerCredentuals is null) {
                storedServerCredentuals = new();
            }

            if (storedServerCredentuals.SavedCredentials is null) {
                storedServerCredentuals.SavedCredentials = new List<SavedCredential>();
            }

            storedServerCredentuals.SavedCredentials.Add(new SavedCredential() {
                Credential = credential,
                ReachableAddress = serverAddress.ToString()
            });

            _secureFileStorageService.StoreModel(storedServerCredentuals);
        }

        public (Uri? serverAddress, string credential) RecoverLogin() {
            var storedServerCredentuals = _secureFileStorageService.GetModel<StoredServerCredentials?>();
            if (storedServerCredentuals is null) {
                return (null, string.Empty);
            }

            var firstSavedCredential = storedServerCredentuals.SavedCredentials.First();
            return (firstSavedCredential.ReachableAddress.NormalizeHostUri(), firstSavedCredential.Credential);
        }
    }
}
