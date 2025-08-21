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
            var credential = new string(oneKey.Take(64).ToArray());
            var addressHex = new string(oneKey.Skip(64).ToArray());

            var serverAddressRegex = new Regex(@"https?:\/\/.+?(?=https?:\/\/|$)");
            var addressesString = Encoding.UTF8.GetString(Convert.FromHexString(addressHex));
            var addresses = serverAddressRegex.Matches(addressesString)
                .SelectMany(i => i.Captures)
                .Skip(1)
                .Select(i => i.Value);

            return (addresses, credential);
        }
    }
}
