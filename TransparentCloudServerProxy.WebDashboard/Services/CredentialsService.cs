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

        public bool AuthenticateCredential(byte[] credential, byte[] savedHash) {
            /*
             The use of the flag allows us to call this function and expect the same runtime speed
             Fail fast would cause this function to be vulnerable to a timing attack vulnerability 
             */

            var acceptedFlag = true;
            var credentialHash = HashCredential(credential);

            if (credentialHash.Length != savedHash.Length) {
                acceptedFlag = false;
            }

            for (var x = 0; x < credentialHash.Length; x++) {
                if (credentialHash[x] != savedHash[x]) {
                    acceptedFlag = false;
                }
            }

            return acceptedFlag;
        }

        public byte[] HashCredential(byte[] credential) {
            using var sha512 = SHA512.Create();
            return sha512.ComputeHash(credential);
        }
    }
}
