using System;
using System.Threading.Tasks;

using TransparentCloudServerProxy.WebDashboard.SqlDb.Models;

namespace TransparentCloudServerProxy.Client.Services.Interfaces {
    public interface IAuthenticationService {
        bool ValidCredentials { get; }

        Task<bool> CheckCredential();
        (Uri?, string?) GetCurrentCredentials();
        ProxyUser? GetCurrentUser();
        Task<bool> LoginAsync(string server, string credential);
    }
}