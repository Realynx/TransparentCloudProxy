namespace TransparentCloudServerProxy.WebDashboard.SqlDb.Models {
    public class AssociatedCredential {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Credential { get; set; }
        public DateTimeOffset ValidTo { get; set; }

        public AssociatedServer AssociatedServer { get; set; }
        public Guid AssociatedServerId { get; set; }
    }
}