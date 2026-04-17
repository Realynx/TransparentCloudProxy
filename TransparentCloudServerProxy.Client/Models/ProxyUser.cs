using System;
using System.Collections.Generic;

using TransparentCloudServerProxy.Models;
using TransparentCloudServerProxy.ProxyBackend;

namespace TransparentCloudServerProxy.WebDashboard.SqlDb.Models {
    public class ProxyUser {
        public Guid Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string HashedCredentialKey { get; set; } = string.Empty;
        public bool Admin { get; set; }
        public DateTime LastLogin { get; set; }
        public List<SavedProxy> UserSavedProxies { get; set; } = new();
    }

    public class SavedProxy {
        public string ListenHost { get; set; } = string.Empty;
        public int ListenPort { get; set; }
        public string TargetHost { get; set; } = string.Empty;
        public int TargetPort { get; set; }
        public ProxySocketType SocketType { get; set; }
        public PacketEngine PacketEngine { get; set; }
        public bool Enabled { get; set; }

        public Proxy GetProxy() {
            return new Proxy(PacketEngine, SocketType, ListenHost, ListenPort, TargetHost, TargetPort) {
                Enabled = Enabled
            };
        }
    }
}
