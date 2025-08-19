
namespace TransparentCloudServerProxy.WebDashboard.Services {
    public interface IPublicAddressService {
        Task<string> GetPublicAddress();
    }
}