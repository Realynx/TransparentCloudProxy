using System.ComponentModel.DataAnnotations;

namespace TransparentCloudServerProxy.WebDashboard.SqlDb.Models {
    public class AssociatedServer {
        public Guid Id { get; set; }

        [Required]
        public string RootCredential { get; set; }

        [Required]
        public string ServerAddress { get; set; }
    }
}
