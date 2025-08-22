using System.Threading;
using System.Threading.Tasks;

using TransparentCloudServerProxy.Client.Models;
using TransparentCloudServerProxy.ProxyBackend;
using TransparentCloudServerProxy.WebDashboard.SqlDb.Models;

namespace TransparentCloudServerProxy.Client.Services.Interfaces {
    public interface IProxyServer {
        string Address { get; }
        SavedCredential SavedCredential { get; }
        ProxyUser ServerUser { get; set; }

        Task<bool> DeleteProxy(Proxy proxy, CancellationToken cancellationToken = default);
        Task<ProxyUser?> GetUser(CancellationToken cancellationToken = default);
        Task<bool> UpdateOrAddProxy(Proxy proxy, CancellationToken cancellationToken = default);
    }
}