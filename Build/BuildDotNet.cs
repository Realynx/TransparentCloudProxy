
using Realynx.CatTail.Attributes;

namespace Build {
    internal class BuildDotNet : IBuildScript {
        private CancellationToken _cancellationToken;
        public void ConfigureBuild(object configure) {

        }

        public Task StartAsync(CancellationToken cancellationToken) {
            _cancellationToken = cancellationToken;

        }

        public Task StopAsync(CancellationToken cancellationToken) {

        }
    }
}
