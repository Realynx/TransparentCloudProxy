using System.Security.Cryptography;
using System.Text;

namespace TransparentCloudServerProxy.WebDashboard.Extentions {
    public static class GuidExtentions {
        public static Guid FromSeed(string seed) {
            var seedBytes = Encoding.UTF8.GetBytes(seed);

            using var sha1 = SHA256.Create();
            var hash = sha1.ComputeHash(seedBytes);

            var guidBytes = new byte[16];
            Array.Copy(hash, guidBytes, 16);

            return new Guid(guidBytes);
        }
    }
}
