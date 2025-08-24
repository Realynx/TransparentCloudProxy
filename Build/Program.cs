using Microsoft.Extensions.Hosting;

using Realynx.CatTail;

namespace Build {
    internal class Program {
        public static void Main(string[] args) {
            var builderHost = new HostBuilder()
                .ConfigureDefaults(args);

            builderHost
                .UseCatTail()
                .AddBuildStage("Test")
                .AddBuildStage("Build")
                .AddBuildStage("Publish");

            var startup = new Startup();
            builderHost.ConfigureAppConfiguration(startup.ConfigureAppConfig);
            builderHost.ConfigureServices(startup.ConfigureServices);



            builderHost.UseConsoleLifetime();
            var host = builderHost.Build();
            host.Run();
        }
    }
}
