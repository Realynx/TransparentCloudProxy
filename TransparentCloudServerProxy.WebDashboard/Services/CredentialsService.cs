using System.Security.Cryptography;

namespace TransparentCloudServerProxy.WebDashboard.Services {
    public class CredentialsService {
        private readonly ILogger<CredentialsService> _logger;

        public CredentialsService(ILogger<CredentialsService> logger) {
            _logger = logger;
        }

        public byte[] GenerateCredential() {
            var data = new byte[32];
            RandomNumberGenerator.Fill(data);

            return data;
        }

        public byte[] HashCredential(byte[] credential) {
            using var sha512 = SHA512.Create();
            return sha512.ComputeHash(credential);
        }
    }
}
