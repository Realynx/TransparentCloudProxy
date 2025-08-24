using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Realynx.CatTail.Services;
using Realynx.CatTail.Services.Interfaces;

namespace Realynx.CatTail {
    public static partial class HostBuilderExtentions {
        public static void UseCatTail(this IHostBuilder hostBuilder) {
            hostBuilder.ConfigureServices(services => {
                services
                    .AddSingleton<IShell, Shell>()
                    .AddSingleton(new SolutionFileReader(""));
            });
        }
    }
}
