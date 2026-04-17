using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

using TransparentCloudServerProxy.Client.Services.Interfaces;

namespace TransparentCloudServerProxy.Client.Services {
    public class OneKeyService : IOneKeyService {
        public OneKeyService() {
        }

        public (IEnumerable<string> reachableAddresses, string credential) DecodeOneKey(string oneKey) {
            if (string.IsNullOrWhiteSpace(oneKey) || oneKey.Length <= 64) {
                return (Array.Empty<string>(), string.Empty);
            }

            var credential = new string(oneKey.Take(64).ToArray());
            var addressHex = new string(oneKey.Skip(64).ToArray());

            var serverAddressRegex = new Regex(@"https?:\/\/.+?(?=https?:\/\/|$)");
            var addressesString = Encoding.UTF8.GetString(Convert.FromHexString(addressHex));

            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var orderedAddresses = new List<string>();

            foreach (var address in serverAddressRegex.Matches(addressesString)
                .SelectMany(i => i.Captures)
                .Select(i => i.Value)) {
                if (!Uri.TryCreate(address, UriKind.Absolute, out var uri)) {
                    continue;
                }

                if (uri.IsDefaultPort) {
                    throw new FormatException($"OneKey address must include an explicit port: {address}");
                }

                var normalized = uri.ToString();
                if (seen.Add(normalized)) {
                    orderedAddresses.Add(normalized);
                }
            }

            return (orderedAddresses, credential);
        }
    }
}
