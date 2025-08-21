using System;

using TransparentCloudServerProxy.Client.Models;

namespace TransparentCloudServerProxy.Client.Services.Interfaces {
    public interface ILoginStorageService {
        SavedCredential[] GetAllLogins();
        (Uri? serverAddress, string credential) RecoverLogin();
        void RemoveLogin(Uri serverAddress);
        void StoreLogin(string credential, Uri serverAddress);
        void StoreLogin(SavedCredential savedCredential);
    }
}