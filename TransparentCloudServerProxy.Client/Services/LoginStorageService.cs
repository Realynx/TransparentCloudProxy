using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

using TransparentCloudServerProxy.Client.Extentions;
using TransparentCloudServerProxy.Client.Services.Interfaces;

namespace TransparentCloudServerProxy.Client.Services {
    public class LoginStorageService : ILoginStorageService {
        private const string CONFIG_NAME = "config.bin";

        public LoginStorageService() {
        }

        public void StoreLogin(string credential, Uri serverAddress) {
            var configString = $"{credential}|{serverAddress.ToString()}";

            var key = GenerateKey(out var password);
            var encryptedCredetials = EncryptBytes(configString, key);

            File.WriteAllBytes($"{password}{CONFIG_NAME}", encryptedCredetials);
        }

        public (Uri? serverAddress, string credential) RecoverLogin() {
            var configFile = Directory.GetFiles(".", "*").FirstOrDefault(i => i.EndsWith(CONFIG_NAME));
            configFile = Path.GetFileName(configFile);

            if (string.IsNullOrWhiteSpace(configFile)) {
                return (null, string.Empty);
            }

            var key = RecoverKey(configFile);
            try {
                var cipherText = File.ReadAllBytes(configFile);
                var credentialString = DecryptBytes(cipherText, key);
                var credentialStringSplit = credentialString.Split("|");

                if (credentialStringSplit.Length != 2) {
                    return (null, string.Empty);
                }

                return (credentialStringSplit[1].NormalizeHostUri(), credentialStringSplit[0]);
            }
            catch {
                return (null, string.Empty);
            }
        }

        private static byte[] RecoverKey(string fileName) {
            var password = fileName.Split(CONFIG_NAME)[0];
            var key = ComputeKey(password);

            return key;
        }

        private static byte[] GenerateKey(out string password) {
            var rng = new Random();
            password = rng.Next(100000, 999999).ToString();

            return ComputeKey(password);
        }

        private static byte[] ComputeKey(string password) {
            var key = new byte[32];
            var sha = SHA256.Create();

            var hash = sha.ComputeHash(Encoding.UTF8.GetBytes(password));

            Array.Copy(hash, key, key.Length);
            return key;
        }

        private byte[] EncryptBytes(string data, byte[] key) {
            using var aes = Aes.Create();
            aes.Key = key;
            aes.GenerateIV();

            using var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

            using var ms = new MemoryStream();
            ms.Write(aes.IV, 0, aes.IV.Length);

            using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
            using (var writer = new StreamWriter(cs, Encoding.UTF8)) {
                writer.Write(data);
            }

            return ms.ToArray();
        }

        private string DecryptBytes(byte[] cipherData, byte[] key) {
            using var aes = Aes.Create();

            var iv = new byte[aes.BlockSize / 8];
            Array.Copy(cipherData, 0, iv, 0, iv.Length);

            aes.Key = key;
            aes.IV = iv;

            using var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
            using var ms = new MemoryStream(cipherData, iv.Length, cipherData.Length - iv.Length);
            using var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
            using var reader = new StreamReader(cs, Encoding.UTF8);

            return reader.ReadToEnd();
        }

    }
}
