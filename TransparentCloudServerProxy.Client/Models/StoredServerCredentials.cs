using System.Collections.Generic;

namespace TransparentCloudServerProxy.Client.Models {
    public class StoredServerCredentials {
        public List<SavedCredential> SavedCredentials { get; set; }
    }

    public class SavedCredential {
        public string ReachableAddress { get; set; }
        public string Credential { get; set; }
    }
}
