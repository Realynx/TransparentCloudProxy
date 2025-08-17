using System.Threading.Tasks;

using TransparentCloudServerProxy.ProxyBackend;

namespace TransparentCloudServerProxy.Client.Services.Interfaces {
    public interface IProxyApi {
        Task<bool> DeleteProxy(Proxy proxy);
        Task<bool> UpdateOrAddProxy(Proxy proxy);
    }
}