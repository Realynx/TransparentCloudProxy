using System.ComponentModel;

using ReactiveUI;
using ReactiveUI.Fody.Helpers;

using TransparentCloudServerProxy.ProxyBackend;

namespace TransparentCloudServerProxy.Client.Models {
    public class AppSettingsModel : ReactiveObject {
        public AppSettingsModel() {

        }

        [Reactive, Category("App Settings"), DisplayName("Theme")]
        public string Theme { get; set; } = "Red";


        [Reactive, Category("App Settings"), DisplayName("Save Logins")]
        public bool SaveLogins { get; set; } = true;

        [Reactive, Category("App Settings"), DisplayName("Saved Login Location")]
        public string LoginCacheFile { get; set; } = "./config.bin";


        [Reactive, Category("Local Proxy"), DisplayName("Default Local Packet Engine")]
        public PacketEngine LocalDefaultEngine { get; set; } = PacketEngine.NativeC;


        [Reactive, Category("Cloud Proxy"), DisplayName("Azure PAT Credential")]
        public string AzurePAT { get; set; } = "";

        [Reactive, Category("Cloud Proxy"), DisplayName("Aws Access Key Credential")]
        public string AwsAccessKey { get; set; } = "";

        [Reactive, Category("Cloud Proxy"), DisplayName("Default Cloud Packet Engine")]
        public PacketEngine DefaultEngine { get; set; } = PacketEngine.NetFilter;

    }
}
