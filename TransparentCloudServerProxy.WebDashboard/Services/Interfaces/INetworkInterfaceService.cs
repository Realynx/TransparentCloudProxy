using System.Net;

namespace TransparentCloudServerProxy.WebDashboard.Services.Interfaces {
    public interface INetworkInterfaceService {
        Task<string> CreateReachableAddressString();
        IPAddress[] GetNetworkInterfaceAddresses();
    }
}