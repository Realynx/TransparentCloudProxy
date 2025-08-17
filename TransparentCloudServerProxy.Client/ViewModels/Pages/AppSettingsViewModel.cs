using ReactiveUI.Fody.Helpers;

using TransparentCloudServerProxy.Client.Models;

namespace TransparentCloudServerProxy.Client.ViewModels.Pages {
    public class AppSettingsViewModel : ViewModel {

        [Reactive]
        public AppSettingsModel AppSettingsModel { get; set; }

        public AppSettingsViewModel(AppSettingsModel appSettingsModel) {
            AppSettingsModel = appSettingsModel;
        }

    }
}
