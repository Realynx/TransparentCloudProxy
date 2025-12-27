using System.ComponentModel;

using CommunityToolkit.Mvvm.ComponentModel;

using TransparentCloudServerProxy.ProxyBackend;

namespace TransparentCloudServerProxy.Client.Models {
    public partial class AppSettingsModel : ObservableObject {
        public AppSettingsModel() {

        }

        [ObservableProperty, Category("App Settings"), DisplayName("Display Name")]
        public partial string DisplayName { get; set; } = "Local User";

        [ObservableProperty, Category("App Settings"), DisplayName("Save Logins")]
        public partial bool SaveLogins { get; set; } = true;


        [ObservableProperty, Category("Local Proxy"), DisplayName("Default Local Packet Engine")]
        public partial PacketEngine LocalDefaultEngine { get; set; } = PacketEngine.NativeC;


        [ObservableProperty, Category("Cloud Proxy"), DisplayName("Azure PAT Credential")]
        public partial string AzurePAT { get; set; } = "";

        [ObservableProperty, Category("Cloud Proxy"), DisplayName("Aws Access Key Credential")]
        public partial string AwsAccessKey { get; set; } = "";

        [ObservableProperty, Category("Cloud Proxy"), DisplayName("Default Cloud Packet Engine")]
        public partial PacketEngine DefaultCloudEngine { get; set; } = PacketEngine.NetFilter;


        [ObservableProperty, Category("Theme"), DisplayName("ThemeName")]
        public partial string ThemeName { get; set; } = "Red";

        [ObservableProperty, Category("Theme"), DisplayName("Style")]
        public partial string ThemeStyle { get; set; } = "flat";

        [ObservableProperty, Category("Theme"), DisplayName("ShadCn")]
        public partial bool ShadCd { get; set; } = false;


        [ObservableProperty, Category("Security"), DisplayName("Config File Location")]
        public partial string LoginCacheFile { get; set; } = "./config.bin";

        [ObservableProperty, Category("Security"), DisplayName("The password to encrypt the config file with. Empty means auto/gen")]
        public partial string ConfigPassword { get; set; } = "";
    }
}
