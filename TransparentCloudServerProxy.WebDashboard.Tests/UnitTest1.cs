using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.Extensions.Hosting;

using Moq;
using Moq.AutoMock;

using TransparentCloudServerProxy.WebDashboard.Models;
using TransparentCloudServerProxy.WebDashboard.Services;

namespace TransparentCloudServerProxy.WebDashboard.Tests {
    public class UnitTest1 {
        [Fact]
        public async Task Test1() {
            var autoMocker = new AutoMocker();
            var httpClientMock = autoMocker.GetMock<IHttpClientFactory>();
            httpClientMock
                .Setup(i => i.CreateClient(It.IsAny<string>()))
                .Returns(new HttpClient());

            var currentKestralServerConfig = new CurrentKestralServerConfig(Mock.Of<IServer>(), Mock.Of<IHostApplicationLifetime>());
            var networkInterfaceService = new NetworkInterfaceService(new PublicAddressService(httpClientMock.Object), currentKestralServerConfig);

            var addressString = await networkInterfaceService.CreateReachableAddressString();
            Console.WriteLine(addressString);
        }
    }
}
