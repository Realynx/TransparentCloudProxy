using System.Threading.Tasks;

using TransparentCloudServerProxy.Client.Models;
using TransparentCloudServerProxy.Client.Services.Api;

namespace TransparentCloudServerProxy.Client.Services.Interfaces {
    public interface IProxyServerService {
        Task<ProxyServer?> AddServer(SavedCredential savedCredential);
        ProxyServer[] GetAllServers();
        ProxyServer? GetServer(string address);
    }
}