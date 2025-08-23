
namespace TransparentCloudServerProxy.WebDashboard.Services.Interfaces {
    public interface IPublicAddressService {
        Task<string> GetPublicAddress();
    }
}