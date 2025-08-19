using System.Net;
using System.Net.NetworkInformation;
using System.Text;

using TransparentCloudServerProxy.WebDashboard.Services.Interfaces;

namespace TransparentCloudServerProxy.WebDashboard.Services {
    public class NetworkInterfaceService : INetworkInterfaceService {
        private readonly IPublicAddressService _publicAddressService;

        public NetworkInterfaceService(IPublicAddressService publicAddressService) {
            _publicAddressService = publicAddressService;
        }

        public IPAddress[] GetNetworkInterfaceAddresses() {
            var networkInterfaces = NetworkInterface.GetAllNetworkInterfaces()
                .Where(i => i.OperationalStatus == OperationalStatus.Up && i.NetworkInterfaceType != NetworkInterfaceType.Loopback);

            var myAddresses = new List<IPAddress>();
            foreach (var networkInterface in networkInterfaces) {
                var localAddresses = networkInterface.GetIPProperties().UnicastAddresses.Select(i => i.Address);
                myAddresses.AddRange(localAddresses);
            }

            return myAddresses.ToArray();
        }

        public async Task<string> CreateReachableAddressString() {
            var allLocalAddresses = GetNetworkInterfaceAddresses();
            var publicAddress = await _publicAddressService.GetPublicAddress();

            var addressString = string.Join(string.Empty, allLocalAddresses.Select(i => $"https://{i}").ToArray());
            addressString += $"https://{publicAddress}";
            var hexInterfaceAddress = Convert.ToHexString(Encoding.UTF8.GetBytes(addressString));
            return hexInterfaceAddress;
        }
    }
}
