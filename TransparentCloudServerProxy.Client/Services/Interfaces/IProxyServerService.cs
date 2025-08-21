using System.Collections.ObjectModel;
using System.Threading.Tasks;

using TransparentCloudServerProxy.Client.Models;
using TransparentCloudServerProxy.Client.Services.Api;

namespace TransparentCloudServerProxy.Client.Services.Interfaces {
    public interface IProxyServerService {
        Task<ProxyServer?> AddServer(SavedCredential savedCredential);
        Task<ProxyServer?> AddServer(string oneKey);
        ProxyServer[] GetAllServers();
        ProxyServer? GetServer(string address);
        ObservableCollection<ProxyServer> GetServerObservableCollection();
        void RemoveServer(ProxyServer proxyServer);
    }
}