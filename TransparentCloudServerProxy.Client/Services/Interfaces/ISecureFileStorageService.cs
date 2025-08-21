namespace TransparentCloudServerProxy.Client.Services.Interfaces {
    public interface ISecureFileStorageService {
        object? this[string modelKey] { get; }

        T? GetModel<T>(string modelKey);
        T? GetModel<T>();
        void StoreModel<T>(string modelKey, T? model);
        void StoreModel<T>(T? model);
    }
}