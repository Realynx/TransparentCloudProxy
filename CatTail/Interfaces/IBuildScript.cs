
namespace Realynx.CatTail.Attributes {
    public interface IBuildScript {
        void ConfigureBuild(object configure);
        Task StartAsync(CancellationToken cancellationToken);
        Task StopAsync(CancellationToken cancellationToken);
    }
}