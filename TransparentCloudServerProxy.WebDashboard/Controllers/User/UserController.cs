using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using TransparentCloudServerProxy.ProxyBackend;
using TransparentCloudServerProxy.WebDashboard.Services;
using TransparentCloudServerProxy.WebDashboard.Services.Interfaces;
using TransparentCloudServerProxy.WebDashboard.SqlDb;
using TransparentCloudServerProxy.WebDashboard.SqlDb.Models;

namespace TransparentCloudServerProxy.WebDashboard.Controllers.User {
    [ApiController]
    [Route("[controller]")]
    [Authorize(AuthenticationSchemes = "UserKeyToken")]
    public class UserController : ControllerBase {
        private readonly IDbContextFactory<WebDashboardDbContext> _dbContextFactory;
        private readonly CredentialsService _credentialsService;
        private readonly IDatabaseProxyService _proxyService;
        private readonly IUserService _userService;

        public UserController(IDbContextFactory<WebDashboardDbContext> dbContextFactory, CredentialsService credentialsService,
            IDatabaseProxyService proxyService, IUserService userService) {
            _dbContextFactory = dbContextFactory;
            _credentialsService = credentialsService;
            _proxyService = proxyService;
            _userService = userService;
        }

        [HttpGet(nameof(Get))]
        public IActionResult Get() {
            using var dbContext = _dbContextFactory.CreateDbContext();

            var currentUser = _userService.GetCurrentUser(User);
            if (currentUser.Admin) {
                currentUser.UserSavedProxies = _proxyService
                    .GetProxies()
                    .Select(i => new SavedProxy((Proxy)i, currentUser.Id))
                    .ToList();
            }

            return Ok(currentUser);
        }


        [HttpPost(nameof(ResetMyCredential))]
        public async Task<IActionResult> ResetMyCredential() {
            using var dbContext = _dbContextFactory.CreateDbContext();
            var currentUser = _userService.GetCurrentUser(User);

            var rootCredential = _credentialsService.GenerateCredential();
            var credentialHash = _credentialsService.HashCredential(rootCredential);
            var hashString = Convert.ToHexString(credentialHash);

            currentUser.HashedCredentialKey = hashString;
            await dbContext.SaveChangesAsync();

            var credentialString = Convert.ToHexString(rootCredential);
            return Ok(credentialString);
        }
    }
}
