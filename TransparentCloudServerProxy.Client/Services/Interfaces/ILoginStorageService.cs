using System;

namespace TransparentCloudServerProxy.Client.Services.Interfaces {
    public interface ILoginStorageService {
        (Uri? serverAddress, string credential) RecoverLogin();
        void StoreLogin(string credential, Uri serverAddress);
    }
}