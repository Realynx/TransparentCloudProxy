using System.ComponentModel;

using ReactiveUI;
using ReactiveUI.Fody.Helpers;

using TransparentCloudServerProxy.ProxyBackend;

namespace TransparentCloudServerProxy.Client.Models {
    public class AppSettingsModel : ReactiveObject {
        public AppSettingsModel() {

        }

        [Reactive, Category("App Settings"), DisplayName("Display Name")]
        public string DisplayName { get; set; } = "Local User";

        [Reactive, Category("App Settings"), DisplayName("Save Logins")]
        public bool SaveLogins { get; set; } = true;


        [Reactive, Category("Local Proxy"), DisplayName("Default Local Packet Engine")]
        public PacketEngine LocalDefaultEngine { get; set; } = PacketEngine.NativeC;


        [Reactive, Category("Cloud Proxy"), DisplayName("Azure PAT Credential")]
        public string AzurePAT { get; set; } = "";

        [Reactive, Category("Cloud Proxy"), DisplayName("Aws Access Key Credential")]
        public string AwsAccessKey { get; set; } = "";

        [Reactive, Category("Cloud Proxy"), DisplayName("Default Cloud Packet Engine")]
        public PacketEngine DefaultCloudEngine { get; set; } = PacketEngine.NetFilter;


        [Reactive, Category("Theme"), DisplayName("ThemeName")]
        public string ThemeName { get; set; } = "Red";

        [Reactive, Category("Theme"), DisplayName("Style")]
        public string ThemeStyle { get; set; } = "flat";

        [Reactive, Category("Theme"), DisplayName("ShadCn")]
        public bool ShadCd { get; set; } = false;


        [Reactive, Category("Security"), DisplayName("Config File Location")]
        public string LoginCacheFile { get; set; } = "./config.bin";

        [Reactive, Category("Security"), DisplayName("The password to encrypt the config file with. Empty means auto/gen")]
        public string ConfigPassword { get; set; } = "";
    }
}
