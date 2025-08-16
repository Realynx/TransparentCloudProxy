using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

using ReactiveUI;

using TransparentCloudServerProxy.Client.Services.Interfaces;
using TransparentCloudServerProxy.ProxyBackend;
using TransparentCloudServerProxy.WebDashboard.SqlDb.Models;

namespace TransparentCloudServerProxy.Client.ViewModels.Pages {
    public class UserControlPanelViewModel : ViewModel {
        private readonly IAuthenticationService _authenticationService;

        private string _currentUsername;
        public string CurrentUsername {
            get => _currentUsername;
            set => this.RaiseAndSetIfChanged(ref _currentUsername, value);
        }

        private ObservableCollection<Proxy> _savedProxies;
        public ObservableCollection<Proxy> SavedProxies {
            get => _savedProxies;
            set => this.RaiseAndSetIfChanged(ref _savedProxies, value);
        }

        public UserControlPanelViewModel(IAuthenticationService authenticationService) {
            _authenticationService = authenticationService;

            Initialize();
        }

        public void Initialize() {
            var currentUser = _authenticationService.GetCurrentUser();
            _currentUsername = currentUser.Username;
            _savedProxies = new ObservableCollection<Proxy>(currentUser.UserSavedProxies
                .Select(i => i.GetProxy())
                .Where(i => i is not null)
                .Cast<Proxy>()
                .ToList());
        }
    }
}
