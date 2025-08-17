using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Threading.Tasks;

using Avalonia.Controls;

using ReactiveUI;
using ReactiveUI.Fody.Helpers;

using TransparentCloudServerProxy.Client.Services.Interfaces;
using TransparentCloudServerProxy.ProxyBackend;
using TransparentCloudServerProxy.WebDashboard.SqlDb.Models;

namespace TransparentCloudServerProxy.Client.ViewModels.Pages {
    public class UserControlPanelViewModel : ViewModel {
        private readonly IAuthenticationService _authenticationService;

        [Reactive]
        public string CurrentUsername { get; set; }

        [Reactive]
        public ObservableCollection<Proxy> DataGridProxies { get; set; }

        [Reactive]
        public ProxyUser CurrentUser { get; set; }

        [Reactive]
        public bool ApplyChangesVisible { get; set; }

        public ReactiveCommand<Unit, Unit> ApplyChanges { get; }
        public ReactiveCommand<Unit, Unit> ResetChanges { get; }



        public UserControlPanelViewModel(IAuthenticationService authenticationService) {
            _authenticationService = authenticationService;

            ResetChanges = ReactiveCommand.CreateFromTask(ResetChangesAsync);
            ApplyChanges = ReactiveCommand.CreateFromTask(ApplyChangesAsync);
            Initialize();
        }

        public void Initialize() {
            var currentUser = _authenticationService.GetCurrentUser();
            CurrentUser = currentUser;
            CurrentUsername = currentUser.Username;

            SetupDataGrid();
        }

        private void SetupDataGrid() {
            DataGridProxies = new ObservableCollection<Proxy>(CurrentUser.UserSavedProxies
                .Select(i => i.GetProxy())
                .Where(i => i is not null)
                .Cast<Proxy>()
                .ToList());

            DataGridProxies.CollectionChanged += (_, _) => ApplyChangesVisible = true;
        }

        public async Task ResetChangesAsync() {
            ApplyChangesVisible = false;
            SetupDataGrid();
        }

        public async Task ApplyChangesAsync() {
            ApplyChangesVisible = false;

            var originalProxies = new Dictionary<string, Proxy>();
            foreach (var proxy in CurrentUser.UserSavedProxies.Select(i => i.GetProxy())) {
                originalProxies.Add($"{proxy.ListenHost}:{proxy.ListenPort}", proxy);
            }

            var proxiesToDelete = new List<Proxy>();
            foreach (var updatedProxy in DataGridProxies) {
                var effectiveKey = $"{updatedProxy.ListenHost}:{updatedProxy.ListenPort}";
                if (originalProxies.ContainsKey(effectiveKey) && updatedProxy != originalProxies[effectiveKey]) {
                    proxiesToDelete.Add(originalProxies[effectiveKey]);
                }
            }

            SetupDataGrid();
        }
    }
}
