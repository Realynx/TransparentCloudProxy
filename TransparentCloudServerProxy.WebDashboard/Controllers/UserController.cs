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

        [HttpGet(nameof(Get))]
        public IActionResult Get() {
            using var dbContext = _dbContextFactory.CreateDbContext();

            if (User.Identity is not ClaimsIdentity identity) {
                return BadRequest();
            }

            var idClaim = identity.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(idClaim) || !Guid.TryParse(idClaim, out var userId)) {
                return BadRequest();
            }

            if (!Guid.TryParse(idClaim, out var currentUserId)) {
                return BadRequest();
            }

            var currentUser = dbContext.Users.Find(currentUserId);
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
    }
}
