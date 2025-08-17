using System.Text.Json;
using System.Text.Json.Serialization;

using TransparentCloudServerProxy.ProxyBackend;
using TransparentCloudServerProxy.WebDashboard.Extentions;

namespace TransparentCloudServerProxy.WebDashboard.SqlDb.Models {
    public class SavedProxy {
        public SavedProxy() {

        }
        public Guid Id { get; set; }
        public Guid ProxyUserId { get; set; }
        [JsonIgnore]
        public ProxyUser ProxyUserOwner { get; set; } = default!;

        public string ProxyBase64Json { get; set; }

        public Proxy? GetProxy() {
            return JsonSerializer.Deserialize<Proxy>(ProxyBase64Json);
        }

        public SavedProxy(Proxy proxy, Guid proxyUserId) {
            Id = GuidExtentions.FromSeed(proxy.ToString());
            ProxyUserId = proxyUserId;

            ProxyBase64Json = JsonSerializer.Serialize(proxy);
        }
    }
}