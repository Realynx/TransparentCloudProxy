using System.Collections.ObjectModel;
using System.Linq;

using ReactiveUI.Fody.Helpers;

using TransparentCloudServerProxy.Client.Services.Interfaces;
using TransparentCloudServerProxy.ProxyBackend;

namespace TransparentCloudServerProxy.Client.ViewModels.Pages {
    public class UserControlPanelViewModel : ViewModel {
        private readonly IAuthenticationService _authenticationService;

        [Reactive]
        public string CurrentUsername { get; set; }

        [Reactive]
        public ObservableCollection<Proxy> SavedProxies { get; set; }

        public UserControlPanelViewModel(IAuthenticationService authenticationService) {
            _authenticationService = authenticationService;

            Initialize();
        }

        public void Initialize() {
            var currentUser = _authenticationService.GetCurrentUser();
            CurrentUsername = currentUser.Username;
            SavedProxies = new ObservableCollection<Proxy>(currentUser.UserSavedProxies
                .Select(i => i.GetProxy())
                .Where(i => i is not null)
                .Cast<Proxy>()
                .ToList());
        }
    }
}
