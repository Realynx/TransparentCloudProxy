using System.Threading.Tasks;

namespace TransparentCloudServerProxy.Client.Services.Interfaces {
    public interface IAuthenticationService {
        Task LoadAllSavedCredentials();
        bool Login(string server, string credential);
    }
}