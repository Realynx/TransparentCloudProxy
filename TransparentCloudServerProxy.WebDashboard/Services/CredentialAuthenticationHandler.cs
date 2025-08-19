using System.Security.Claims;
using System.Security.Principal;
using System.Text.Encodings.Web;

using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

using TransparentCloudServerProxy.WebDashboard.SqlDb;
using TransparentCloudServerProxy.WebDashboard.SqlDb.Models;

namespace TransparentCloudServerProxy.WebDashboard.Services {
    public class CredentialAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions> {
        private readonly CredentialsService _credentialsService;
        private readonly IDbContextFactory<WebDashboardDbContext> _dbContextFactory;

        public CredentialAuthenticationHandler(IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger,
            UrlEncoder encoder, CredentialsService credentialsService, IDbContextFactory<WebDashboardDbContext> dbContextFactory)
            : base(options, logger, encoder) {
            _credentialsService = credentialsService;
            _dbContextFactory = dbContextFactory;
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync() {
            if (!Request.Headers.TryGetValue("Authorization", out var header)) {
                return AuthenticateResult.Fail("Missing Authorization Header");
            }

            var key = header.ToString().Replace("Key", string.Empty)
                .Trim();

            if (key.Length % 2 != 0) {
                return AuthenticateResult.Fail("Invalid credentials");
            }

            var givenKey = Convert.FromHexString(key);
            var givenCredentialHash = _credentialsService.HashCredential(givenKey);
            var givenCredentialHashString = Convert.ToHexString(givenCredentialHash);

            using var dbContext = _dbContextFactory.CreateDbContext();

            var clusterResult = await EvauateClusterAuthentication(key, dbContext);
            if (clusterResult.Succeeded) {
                return clusterResult;
            }

            return await EvaluateUserAuthentication(givenCredentialHashString, dbContext);
        }

        private async Task<AuthenticateResult> EvauateClusterAuthentication(string key, WebDashboardDbContext dbContext) {
            var selfServer = await dbContext.AssociatedServers.FirstOrDefaultAsync(i => i.IsSelf);
            if (selfServer is null) {
                return AuthenticateResult.Fail("Invalid credentials");
            }

            var credential = selfServer.AssociatedCredential;
            if (credential.SingleOrDefault(i => i.Credential == key) is not AssociatedCredential serverCredential) {
                return AuthenticateResult.Fail("Invalid credentials");
            }

            if (serverCredential.ValidTo < DateTimeOffset.Now) {
                return AuthenticateResult.Fail("Invalid credentials");
            }

            PruneExpiresCredentials(selfServer);

            var claims = new[] {
                new Claim(ClaimTypes.Role, "ClusterJoiner")
            };

            var identity = new ClaimsIdentity(claims, Scheme.Name);
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, Scheme.Name);

            return AuthenticateResult.Success(ticket);
        }

        private static void PruneExpiresCredentials(AssociatedServer? selfServer) {
            var removeList = selfServer.AssociatedCredential.Where(i => i.ValidTo < DateTimeOffset.Now).ToArray();
            foreach (var removeItem in removeList) {
                selfServer.AssociatedCredential.Remove(removeItem);
            }
        }

        private async Task<AuthenticateResult> EvaluateUserAuthentication(string givenCredentialHashString, WebDashboardDbContext dbContext) {
            var user = await dbContext.Users.FirstOrDefaultAsync(proxyUser =>
                proxyUser.HashedCredentialKey == givenCredentialHashString);

            if (user == null) {
                return AuthenticateResult.Fail("Invalid credentials");
            }

            user.LastLogin = DateTimeOffset.Now;
            await dbContext.SaveChangesAsync();

            var claims = new[] {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Role, user.Admin ? "Admin" : "User")
            };

            var identity = new ClaimsIdentity(claims, Scheme.Name);
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, Scheme.Name);

            return AuthenticateResult.Success(ticket);
        }
    }
}

