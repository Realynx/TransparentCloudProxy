using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Realynx.CatTail;
using Realynx.CatTail.Attributes;

namespace Build {
    internal class Program {
        public static void Main(string[] args) {
            var builderHost = new HostBuilder();

            builderHost
                .UseCatTail()
                .AddBuildStage("Test")
                .AddBuildStage("Compile")
                .AddBuildStage("Pack");

            builderHost.UseConsoleLifetime();
            var host = builderHost.Build();


            host.Run();
        }

    }
}
