using System.Security.Claims;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Primitives;

using TransparentCloudServerProxy.WebDashboard.Services;
using TransparentCloudServerProxy.WebDashboard.SqlDb;
using TransparentCloudServerProxy.WebDashboard.SqlDb.Models;

namespace TransparentCloudServerProxy.WebDashboard.Controllers {
    [ApiController]
    [Route("[controller]")]
    [Authorize(AuthenticationSchemes = "KeyToken")]
    public class UserController : ControllerBase {
        private readonly IDbContextFactory<WebDashboardDbContext> _dbContextFactory;
        private readonly CredentialsService _credentialsService;

        public UserController(IDbContextFactory<WebDashboardDbContext> dbContextFactory, CredentialsService credentialsService) {
            _dbContextFactory = dbContextFactory;
            _credentialsService = credentialsService;
        }

        private ProxyUser? GetCurrentUser(WebDashboardDbContext dbContext) {

            if (User.Identity is not ClaimsIdentity identity) {
                return null;
            }

            var idClaim = identity.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(idClaim) || !Guid.TryParse(idClaim, out var userId)) {
                return null;
            }

            if (!Guid.TryParse(idClaim, out var currentUserId)) {
                return null;
            }

            var currentUser = dbContext.Users
                .Include(i => i.UserSavedProxies)
                .FirstOrDefault(i => i.Id == currentUserId);
            return currentUser;
        }

        [HttpGet(nameof(Get))]
        public IActionResult Get() {
            using var dbContext = _dbContextFactory.CreateDbContext();
            var currentUser = GetCurrentUser(dbContext);
            if (currentUser is null) {
                return BadRequest();
            }

            return Ok(currentUser);
        }

        [HttpPost(nameof(CreateUser))]
        public async Task<IActionResult> CreateUser([FromBody] ProxyUser proxyUser) {
            if (proxyUser.Id != Guid.Empty
                || proxyUser.Admin == true
                || !string.IsNullOrWhiteSpace(proxyUser.HashedCredentialKey)) {
                return BadRequest();
            }

            using var dbContext = _dbContextFactory.CreateDbContext();
            if (dbContext.Users.Any(i => i.Username == proxyUser.Username)) {
                return BadRequest("Username exists.");
            }

            var rootCredential = _credentialsService.GenerateCredential();
            var credentialHash = _credentialsService.HashCredential(rootCredential);
            var hashString = Convert.ToHexString(credentialHash);

            var createdProxyUser = new ProxyUser() {
                Id = Guid.NewGuid(),
                Username = proxyUser.Username,
                HashedCredentialKey = hashString,
                LastLogin = DateTimeOffset.MinValue
            };

            dbContext.Users.Add(createdProxyUser);
            await dbContext.SaveChangesAsync();

            // Onetime send the user their auth key. We only keep the hash of this key.
            var credentialString = Convert.ToHexString(rootCredential);
            Response.Headers.Authorization = new StringValues($"Key {credentialString}");
            return Ok(createdProxyUser);
        }

        [HttpPost(nameof(ResetCredential))]
        public async Task<IActionResult> ResetCredential([FromBody] ProxyUser proxyUser) {
            using var dbContext = _dbContextFactory.CreateDbContext();
            var currentUser = GetCurrentUser(dbContext);
            if (currentUser is null) {
                return BadRequest();
            }

            if (!currentUser.Admin && proxyUser.Username != currentUser.Username) {
                return BadRequest("You can only reset your own credential.");
            }

            var resetUser = dbContext.Users.Single(i => i.Username == proxyUser.Username);

            var rootCredential = _credentialsService.GenerateCredential();
            var credentialHash = _credentialsService.HashCredential(rootCredential);
            var hashString = Convert.ToHexString(credentialHash);

            resetUser.HashedCredentialKey = hashString;
            await dbContext.SaveChangesAsync();

            var credentialString = Convert.ToHexString(rootCredential);
            return Ok(credentialString);
        }


        [HttpPost(nameof(ResetMyCredential))]
        public async Task<IActionResult> ResetMyCredential() {
            using var dbContext = _dbContextFactory.CreateDbContext();
            var currentUser = GetCurrentUser(dbContext);
            if (currentUser is null) {
                return BadRequest();
            }

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
