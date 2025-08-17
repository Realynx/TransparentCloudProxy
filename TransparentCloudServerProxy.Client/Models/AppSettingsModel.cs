using System.ComponentModel;

using ReactiveUI;
using ReactiveUI.Fody.Helpers;

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


        [Reactive, Category("Proxy"), DisplayName("Default Packet Engine")]
        public string DefaultEngine { get; set; } = "NativeC";

    }
}
