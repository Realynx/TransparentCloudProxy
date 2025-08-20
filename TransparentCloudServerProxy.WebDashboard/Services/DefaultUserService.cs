using System.Text.Json;

using Microsoft.EntityFrameworkCore;

using TransparentCloudServerProxy.WebDashboard.Services.Interfaces;
using TransparentCloudServerProxy.WebDashboard.SqlDb;
using TransparentCloudServerProxy.WebDashboard.SqlDb.Models;

namespace TransparentCloudServerProxy.WebDashboard.Services
{
    public class DefaultUserService : IHostedService
    {
        private readonly IHostApplicationLifetime _lifetime;

        /// <summary>
        /// If the account was just created this will contain it's credentials.
        /// </summary>
        public string CredentialString { get; set; }

        private readonly ILogger<DefaultUserService> _logger;
        private readonly CredentialsService _credentialsService;
        private readonly IDbContextFactory<WebDashboardDbContext> _dbContextFactory;
        private readonly INetworkInterfaceService _networkInterfaceService;

        public DefaultUserService(ILogger<DefaultUserService> logger, CredentialsService credentialsService,
            IDbContextFactory<WebDashboardDbContext> dbContextFactory, INetworkInterfaceService networkInterfaceService, IHostApplicationLifetime lifetime)
        {
            _logger = logger;
            _credentialsService = credentialsService;
            _dbContextFactory = dbContextFactory;
            _networkInterfaceService = networkInterfaceService;
            _lifetime = lifetime;
        }

        public async Task<ProxyUser> EnsureDefaultUser()
        {
            using var dbContext = _dbContextFactory.CreateDbContext();
            dbContext.Database.EnsureCreated();

            var existingRootUser = dbContext.Users.SingleOrDefault(i => i.Username == "root");
            if (existingRootUser is not null)
            {
                return existingRootUser;
            }

            var rootCredential = _credentialsService.GenerateCredential();
            CredentialString = Convert.ToHexString(rootCredential);

            var credentialHash = _credentialsService.HashCredential(rootCredential);
            var hashString = Convert.ToHexString(credentialHash);

            existingRootUser = new ProxyUser()
            {
                Id = Guid.NewGuid(),
                Username = "root",
                Admin = true,
                HashedCredentialKey = hashString,
                LastLogin = DateTimeOffset.MinValue
            };

            dbContext.Users.Add(existingRootUser);
            await dbContext.SaveChangesAsync();

            return existingRootUser;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _lifetime.ApplicationStarted.Register(async () => await RegisteredStart());
            return Task.CompletedTask;
        }

        private async Task RegisteredStart()
        {
            var proxyUser = await EnsureDefaultUser();

            if (!string.IsNullOrWhiteSpace(CredentialString))
            {
                _logger.LogInformation("Created root user! Below is your credential, DO NOT LOSE THIS.");
                _logger.LogInformation($"Root Cred: {CredentialString}");

                var hexInterfaceAddress = await _networkInterfaceService.CreateReachableAddressString();

                var oneKey = $"{CredentialString}{hexInterfaceAddress}";
                _logger.LogInformation($"OneKey Pass: {oneKey}");
            }

            var proxyUserJson = JsonSerializer.Serialize(proxyUser);
            _logger.LogInformation($"root user:\n\r{proxyUserJson}");
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
