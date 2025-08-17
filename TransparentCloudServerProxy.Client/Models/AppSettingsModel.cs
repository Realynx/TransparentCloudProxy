using System.ComponentModel;

using ReactiveUI.Fody.Helpers;

namespace TransparentCloudServerProxy.Client.Models {
    public class AppSettingsModel {
        public AppSettingsModel() {

        }

        [Reactive]
        [property: Category("App Settings"), DisplayName("Theme")]
        public string Theme { get; set; } = "Red";

        [Reactive]
        [property: Category("App Settings"), DisplayName("Save Logins")]
        public bool SaveLogins { get; set; } = true;

        [Reactive]
        [property: Category("App Settings"), DisplayName("Saved Login Location")]
        public string LoginCacheFile { get; set; } = "./config.bin";


        [Reactive]
        [property: Category("Proxy"), DisplayName("Default Packet Engine")]
        public string DefaultEngine { get; set; } = "NativeC";

    }
}
