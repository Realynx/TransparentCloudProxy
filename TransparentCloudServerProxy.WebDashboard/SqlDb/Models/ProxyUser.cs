using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

using Microsoft.EntityFrameworkCore;

namespace TransparentCloudServerProxy.WebDashboard.SqlDb.Models {
    [Index(nameof(Username))]
    public class ProxyUser {
        public Guid Id { get; set; }
        public bool Admin { get; set; }
        [JsonRequired]
        public string Username { get; set; }

        [JsonIgnore]
        public string? HashedCredentialKey { get; set; }
        public DateTimeOffset LastLogin { get; set; }
        public List<SavedProxy> UserSavedProxies { get; set; } = new();
    }
}
