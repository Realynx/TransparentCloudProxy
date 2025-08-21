using System.Collections.Generic;

namespace TransparentCloudServerProxy.Client.Services.Interfaces {
    public interface IOneKeyService {
        (IEnumerable<string> reachableAddresses, string credential) DecodeOneKey(string oneKey);
    }
}