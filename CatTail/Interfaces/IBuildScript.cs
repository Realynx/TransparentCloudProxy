
namespace Realynx.CatTail.Attributes {
    internal interface IBuildScript {
        void ConfigureBuild(object configure);
        Task StartAsync(CancellationToken cancellationToken);
        Task StopAsync(CancellationToken cancellationToken);
    }
}