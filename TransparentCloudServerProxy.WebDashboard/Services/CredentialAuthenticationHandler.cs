using System.Security.Claims;
using System.Text.Encodings.Web;

using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

using TransparentCloudServerProxy.WebDashboard.SqlDb;

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

