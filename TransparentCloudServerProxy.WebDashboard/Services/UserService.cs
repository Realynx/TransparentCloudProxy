using System.Security.Claims;

using Microsoft.EntityFrameworkCore;

using TransparentCloudServerProxy.WebDashboard.Services.Interfaces;
using TransparentCloudServerProxy.WebDashboard.SqlDb;
using TransparentCloudServerProxy.WebDashboard.SqlDb.Models;

namespace TransparentCloudServerProxy.WebDashboard.Services {
    public class UserService : IUserService {
        private readonly IDbContextFactory<WebDashboardDbContext> _dbContextFactory;

        public UserService(IDbContextFactory<WebDashboardDbContext> dbContextFactory) {
            _dbContextFactory = dbContextFactory;
        }

        public ProxyUser? GetCurrentUserOrDefault(ClaimsPrincipal user) {

            if (user.Identity is not ClaimsIdentity identity) {
                return null;
            }

            var idClaim = identity.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(idClaim) || !Guid.TryParse(idClaim, out var userId)) {
                return null;
            }

            using var dbContext = _dbContextFactory.CreateDbContext();
            var currentUser = dbContext.Users
                .Include(i => i.UserSavedProxies)
                .FirstOrDefault(i => i.Id == userId);

            return currentUser;
        }

        public ProxyUser GetCurrentUser(ClaimsPrincipal user) {

            if (user.Identity is not ClaimsIdentity identity) {
                throw new Exception("Identity claims are not present");
            }

            var idClaim = identity.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(idClaim) || !Guid.TryParse(idClaim, out var userId)) {
                throw new Exception("No valid ID found");
            }

            using var dbContext = _dbContextFactory.CreateDbContext();
            var currentUser = dbContext.Users
                .Include(i => i.UserSavedProxies)
                .First(i => i.Id == userId);

            return currentUser;
        }
    }
}
