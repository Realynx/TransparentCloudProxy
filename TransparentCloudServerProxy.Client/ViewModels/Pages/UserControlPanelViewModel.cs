using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Threading.Tasks;

using Avalonia.Controls;

using ReactiveUI;
using ReactiveUI.Fody.Helpers;

using Tmds.DBus.Protocol;

using TransparentCloudServerProxy.Client.Services.Api;
using TransparentCloudServerProxy.Client.Services.Interfaces;
using TransparentCloudServerProxy.ProxyBackend;
using TransparentCloudServerProxy.WebDashboard.SqlDb.Models;

namespace TransparentCloudServerProxy.Client.ViewModels.Pages {
    public class UserControlPanelViewModel : ViewModel {
        private readonly IAuthenticationService _authenticationService;
        private readonly IProxyApi _proxyApi;
        private readonly IUserApi _userApi;

        [Reactive]
        public string CurrentUsername { get; set; }

        [Reactive]
        public Proxy SelectedProxy { get; set; }

        [Reactive]
        public ObservableCollection<Proxy> DataGridProxies { get; set; }

        [Reactive]
        public ProxyUser CurrentUser { get; set; }

        [Reactive]
        public bool ApplyChangesVisible { get; set; }



        public ReactiveCommand<Unit, Unit> AddCommand { get; }
        public ReactiveCommand<Unit, Unit> DeleteCommand { get; }
        public ReactiveCommand<Unit, Unit> ApplyChanges { get; }
        public ReactiveCommand<Unit, Unit> ResetChanges { get; }



        public UserControlPanelViewModel(IAuthenticationService authenticationService, IProxyApi proxyApi, IUserApi userApi) {
            _authenticationService = authenticationService;
            _proxyApi = proxyApi;
            _userApi = userApi;
            AddCommand = ReactiveCommand.CreateFromTask(AddCommandAsync);
            DeleteCommand = ReactiveCommand.CreateFromTask(DeleteCommandAsync);
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

        private async Task SetupDataGrid() {
            (var address, var credential) = _authenticationService.GetCurrentCredentials();
            var currentUpdatedUser = await _userApi.Login(address, credential);

            DataGridProxies = new ObservableCollection<Proxy>(currentUpdatedUser.UserSavedProxies
                .Select(i => i.GetProxy())
                .Where(i => i is not null)
                .Cast<Proxy>()
                .ToList());

            DataGridProxies.CollectionChanged += (_, _) => ApplyChangesVisible = true;
        }

        public async Task AddCommandAsync() {
            DataGridProxies.Add(new Proxy(PacketEngine.NativeC, Managed.Models.ProxySocketType.Tcp, "0.0.0.0", 443, "10.0.0.1", 443));
            ApplyChangesVisible = true;
        }

        public async Task DeleteCommandAsync() {
            await _proxyApi.DeleteProxy(SelectedProxy);
            await SetupDataGrid();
        }

        public async Task ResetChangesAsync() {
            ApplyChangesVisible = false;
            await SetupDataGrid();
        }

        public async Task ApplyChangesAsync() {
            ApplyChangesVisible = false;

            var originalProxies = new Dictionary<string, Proxy>();
            foreach (var proxy in CurrentUser.UserSavedProxies.Select(i => i.GetProxy())) {
                originalProxies.Add($"{proxy.ListenHost}:{proxy.ListenPort}", proxy);
            }

            foreach (var updatedProxy in DataGridProxies) {
                var effectiveKey = $"{updatedProxy.ListenHost}:{updatedProxy.ListenPort}";
                if (originalProxies.ContainsKey(effectiveKey)) {
                    originalProxies.Remove(effectiveKey);
                }

                await _proxyApi.UpdateOrAddProxy(updatedProxy);
            }

            foreach (var deletedProxy in originalProxies.Values) {
                await _proxyApi.DeleteProxy(deletedProxy);
            }

            await SetupDataGrid();
        }
    }
}

