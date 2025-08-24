using Realynx.CatTail.Attributes;

namespace Realynx.CatTail {
    internal class ExampleService : IBuildScript {
        public void ConfigureBuild(object configure) {

        }

        public Task StartAsync(CancellationToken cancellationToken) {
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken) {
            return Task.CompletedTask;
        }
    }
}
