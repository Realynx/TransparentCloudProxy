using System.Net;

namespace TransparentCloudServerProxy.WebDashboard.Services.Windows.Interfaces {
    public interface INetworkInterfaceService {
        Task<string> CreateReachableAddressString();
        IPAddress[] GetNetworkInterfaceAddresses();
    }
}