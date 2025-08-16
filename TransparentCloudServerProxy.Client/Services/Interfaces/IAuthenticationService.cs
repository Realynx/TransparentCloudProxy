using System.Threading.Tasks;

namespace TransparentCloudServerProxy.Client.Services.Interfaces {
    public interface IAuthenticationService {
        bool ValidCredentials { get; }

        Task<bool> CheckCredential();
        Task<bool> LoginAsync(string server, string credential);
    }
}