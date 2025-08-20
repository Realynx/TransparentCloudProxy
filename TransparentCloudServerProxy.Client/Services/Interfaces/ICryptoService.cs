namespace TransparentCloudServerProxy.Client.Services.Interfaces {
    public interface ICryptoService {
        byte[] ComputeKey(string password);
        string DecryptBytes(byte[] cipherData, byte[] key);
        byte[] EncryptBytes(string data, byte[] key);
        byte[] GenerateKey(out string password);
        string ReadAllEncryptedBytes(string filePath, string password);
        void WriteAllEncryptedBytes(string filePath, string fileContent, string password);
    }
}