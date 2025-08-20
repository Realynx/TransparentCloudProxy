using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

using TransparentCloudServerProxy.Client.Services.Interfaces;

namespace TransparentCloudServerProxy.Client.Services {
    public class CryptoService : ICryptoService {
        public CryptoService() {

        }

        public string ReadAllEncryptedBytes(string filePath, string password) {
            if (!File.Exists(filePath)) {
                return string.Empty;
            }

            var keyHash = ComputeKey(password);
            var cipherText = File.ReadAllBytes(filePath);
            var plainText = DecryptBytes(cipherText, keyHash);

            return plainText;
        }

        public void WriteAllEncryptedBytes(string filePath, string fileContent, string password) {
            var keyHash = ComputeKey(password);
            var cipherText = EncryptBytes(fileContent, keyHash);
            File.WriteAllBytes(filePath, cipherText);
        }

        public byte[] EncryptBytes(string data, byte[] key) {
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

        public string DecryptBytes(byte[] cipherData, byte[] key) {
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

        public byte[] GenerateKey(out string password) {
            var rng = new Random();
            password = rng.Next(100000, 999999).ToString();

            return ComputeKey(password);
        }

        public byte[] ComputeKey(string password) {
            var key = new byte[32];
            var sha = SHA256.Create();

            var hash = sha.ComputeHash(Encoding.UTF8.GetBytes(password));

            Array.Copy(hash, key, key.Length);
            return key;
        }
    }
}
