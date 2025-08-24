using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Realynx.CatTail.Services;

namespace Realynx.CatTail {
    public static partial class HostBuilderExtentions {
        public static CatTailOptions UseCatTail(this IHostBuilder hostBuilder) {
            hostBuilder.ConfigureServices(services => {
                services
                    .AddSingleton<IShell, Shell>()
                    .AddSingleton(new SolutionFileReader(""));

                return new CatTailOptions();
            });
        }
    }
}
