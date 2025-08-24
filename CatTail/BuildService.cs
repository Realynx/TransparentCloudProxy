using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Realynx.CatTail.Attributes;

namespace Realynx.CatTail {
    internal class BuildService : IHostedService {
        private readonly IServiceProvider _serviceProvider;

        public BuildService(IServiceProvider serviceProvider) {
            _serviceProvider = serviceProvider;
        }

        public Task StartAsync(CancellationToken cancellationToken) {
            var allBuldScripts = _serviceProvider.GetServices<IBuildScript>();

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken) {
            return Task.CompletedTask;
        }
    }
}
