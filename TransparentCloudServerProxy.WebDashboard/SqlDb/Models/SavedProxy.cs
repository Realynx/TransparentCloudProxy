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

        public string SavedProxyJson { get; set; }

        public Proxy? GetProxy() {
            return JsonSerializer.Deserialize<Proxy>(SavedProxyJson);
        }

        public SavedProxy(Proxy proxy, Guid proxyUserId) {
            Id = GuidExtentions.FromSeed($"{proxy.ListenHost}:{proxy.ListenPort}");
            ProxyUserId = proxyUserId;

            SavedProxyJson = JsonSerializer.Serialize(proxy);
        }
    }
}