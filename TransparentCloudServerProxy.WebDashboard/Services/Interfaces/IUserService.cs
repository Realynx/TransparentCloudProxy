using System.Security.Claims;

using TransparentCloudServerProxy.WebDashboard.SqlDb.Models;

namespace TransparentCloudServerProxy.WebDashboard.Services.Interfaces {
    public interface IUserService {
        ProxyUser GetCurrentUser(ClaimsPrincipal user);
        ProxyUser? GetCurrentUserOrDefault(ClaimsPrincipal user);
    }
}