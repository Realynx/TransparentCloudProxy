using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Text.Json;

using TransparentCloudServerProxy.Client.Services.Interfaces;

namespace TransparentCloudServerProxy.Client.Services {
    public class SecureFileStorageService : ISecureFileStorageService {
        private const string CONFIG_NAME = "config.bin";

        private readonly ICryptoService _cryptoService;
        private readonly Random _rng = new();
        private ConcurrentDictionary<string, object?> _configValues = new();

        public SecureFileStorageService(ICryptoService cryptoService) {
            _cryptoService = cryptoService;
            LoadMainConfigFile();
        }

        public object? this[string modelKey] {
            get {
                if (!_configValues.TryGetValue(modelKey, out var value)) {
                    return null;
                }

                return value;
            }
        }

        public void StoreModel<T>(string modelKey, T? model) {
            _configValues[modelKey] = model;
            SaveMainConfigFile();
        }

        public T? GetModel<T>(string modelKey) {
            if (!_configValues.TryGetValue(modelKey, out var value)) {
                return default;
            }

            if (value is JsonElement element) {
                return element.Deserialize<T>();
            }

            return (T?)value;
        }

        public void StoreModel<T>(T? model) {
            StoreModel(typeof(T).Name, model);
        }

        public T? GetModel<T>() {
            return GetModel<T>(typeof(T).Name);
        }

        private void LoadMainConfigFile() {
            var configFile = Directory.GetFiles(".", "*").FirstOrDefault(i => i.EndsWith(CONFIG_NAME));
            if (string.IsNullOrWhiteSpace(configFile)) {
                _configValues = new ConcurrentDictionary<string, object?>();
                SaveMainConfigFile();
                return;
            }

            configFile = Path.GetFileName(configFile);
            var password = configFile.Split(CONFIG_NAME)[0];
            var configFilePlainText = _cryptoService.ReadAllEncryptedBytes(configFile, password);

            // TODO: Handle JsonException and configStore = null
            var configStorage = JsonSerializer.Deserialize<ConcurrentDictionary<string, object?>>(configFilePlainText);
            _configValues = configStorage;
        }

        private void SaveMainConfigFile() {
            var password = _rng.Next(1000000, 99999999).ToString();
            var configFile = Path.GetFullPath($"{password}{CONFIG_NAME}");

            var existingConfigFile = Directory.GetFiles(".", "*").FirstOrDefault(i => i.EndsWith(CONFIG_NAME));
            if (!string.IsNullOrWhiteSpace(existingConfigFile)) {
                configFile = Path.GetFileName(existingConfigFile);
                password = configFile.Split(CONFIG_NAME)[0];
            }

            var plainText = JsonSerializer.Serialize(_configValues);
            _cryptoService.WriteAllEncryptedBytes(configFile, plainText, password);
        }
    }
}
