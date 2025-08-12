using Microsoft.Extensions.Configuration;

namespace TransparentCloudServerProxy.WebDashboard.Models {
    public class DashboardConfig {
        public DashboardConfig(IConfigurationRoot configurationRoot) {
            configurationRoot.GetSection(nameof(DashboardConfig)).Bind(this);
        }

        public string AdminUsername { get; set; }
        public string AdminPassword { get; set; }
        public string ApiToken { get; set; }
    }
}
