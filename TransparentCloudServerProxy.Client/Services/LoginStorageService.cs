using System;
using System.Collections.Generic;
using System.Linq;

using TransparentCloudServerProxy.Client.Extentions;
using TransparentCloudServerProxy.Client.Models;
using TransparentCloudServerProxy.Client.Services.Interfaces;

namespace TransparentCloudServerProxy.Client.Services {
    public class LoginStorageService : ILoginStorageService {
        private readonly ISecureFileStorageService _secureFileStorageService;
        private readonly IOneKeyService _oneKeyService;

        public LoginStorageService(ISecureFileStorageService secureFileStorageService, IOneKeyService oneKeyService) {
            _secureFileStorageService = secureFileStorageService;
            _oneKeyService = oneKeyService;
        }

        public void StoreLogin(string credential, Uri serverAddress) {
            var savedCredential = new SavedCredential() {
                Credential = credential,
                ReachableAddress = serverAddress.ToString()
            };

            StoreLogin(savedCredential);
        }

        public void StoreLogin(SavedCredential savedCredential) {
            var storedServerCredentials = _secureFileStorageService.GetModel<StoredServerCredentials>();

            storedServerCredentials ??= new();
            storedServerCredentials.SavedCredentials ??= new List<SavedCredential>();

            storedServerCredentials.SavedCredentials.Add(savedCredential);

            _secureFileStorageService.StoreModel(storedServerCredentials);
        }

        public void RemoveLogin(Uri serverAddress) {
            var storedServerCredentials = _secureFileStorageService.GetModel<StoredServerCredentials>();
            if (storedServerCredentials is null || storedServerCredentials.SavedCredentials is null) {
                return;
            }

            var existingCredential = storedServerCredentials.SavedCredentials.FirstOrDefault(i => i.ReachableAddress == serverAddress.ToString());
            if (existingCredential is null) {
                return;
            }

            storedServerCredentials.SavedCredentials.Remove(existingCredential);
            _secureFileStorageService.StoreModel(storedServerCredentials);
        }

        public SavedCredential[] GetAllLogins() {
            var storedServerCredentials = _secureFileStorageService.GetModel<StoredServerCredentials>();

            return storedServerCredentials is null || storedServerCredentials.SavedCredentials is null
                ? Array.Empty<SavedCredential>()
                : storedServerCredentials.SavedCredentials.ToArray();
        }

        public (Uri? serverAddress, string credential) RecoverLogin() {
            var storedServerCredentials = _secureFileStorageService.GetModel<StoredServerCredentials?>();
            if (storedServerCredentials is null) {
                return (null, string.Empty);
            }

            var firstSavedCredential = storedServerCredentials.SavedCredentials.First();
            return (firstSavedCredential.ReachableAddress.NormalizeHostUri(), firstSavedCredential.Credential);
        }
    }
}
