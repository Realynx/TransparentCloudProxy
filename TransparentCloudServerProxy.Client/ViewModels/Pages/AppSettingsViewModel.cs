using CommunityToolkit.Mvvm.ComponentModel;

using TransparentCloudServerProxy.Client.Models;

namespace TransparentCloudServerProxy.Client.ViewModels.Pages {
    public partial class AppSettingsViewModel : ViewModel {

        [ObservableProperty]
        public partial AppSettingsModel AppSettingsModel { get; set; }

        public AppSettingsViewModel(AppSettingsModel appSettingsModel) {
            AppSettingsModel = appSettingsModel;
        }

    }
}
