using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Realynx.CatTail.Services;
using Realynx.CatTail.Services.Interfaces;

namespace Realynx.CatTail {
    public static partial class HostBuilderExtensions {
        public static CatTailOptions UseCatTail(this IHostBuilder hostBuilder) {
            hostBuilder.ConfigureServices((context, services) => {
                services
                    .AddSingleton<IShell, Shell>()
                    .AddSingleton(new SolutionFileReader(""));
            });
            return new CatTailOptions();
        }
    }
}
