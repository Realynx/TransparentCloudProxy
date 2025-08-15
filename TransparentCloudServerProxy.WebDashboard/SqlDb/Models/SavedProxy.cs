namespace TransparentCloudServerProxy.WebDashboard.SqlDb.Models {
    public class SavedProxy {
        public Guid Id { get; set; }
        public Guid ProxyUserId { get; set; }
        public ProxyUser ProxyUserOwner { get; set; } = default!;

        public string ProxyBase64Json { get; set; }
    }
}