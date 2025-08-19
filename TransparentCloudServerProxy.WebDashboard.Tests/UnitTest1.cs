using Moq;
using Moq.AutoMock;

using TransparentCloudServerProxy.WebDashboard.Services;
using TransparentCloudServerProxy.WebDashboard.Services.Windows;

namespace TransparentCloudServerProxy.WebDashboard.Tests {
    public class UnitTest1 {
        [Fact]
        public async Task Test1() {
            var autoMocker = new AutoMocker();
            var httpClientMock = autoMocker.GetMock<IHttpClientFactory>();
            httpClientMock
                .Setup(i => i.CreateClient(It.IsAny<string>()))
                .Returns(new HttpClient());

            var networkInterfaceService = new NetworkInterfaceService(new PublicAddressService(httpClientMock.Object));

            var addressString = await networkInterfaceService.CreateReachableAddressString();
            Console.WriteLine(addressString);
        }
    }
}
