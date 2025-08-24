using System.Reflection;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Realynx.CatTail.Generators.Generated;

namespace Build {
    internal partial class Startup {
        private IConfiguration _configuration;

        public void ConfigureAppConfig(IConfigurationBuilder configurationBuilder) {
            configurationBuilder
                .AddUserSecrets(Assembly.GetExecutingAssembly())
                .AddEnvironmentVariables();

            _configuration = configurationBuilder.Build();
        }

        public void ConfigureServices(IServiceCollection serviceDescriptors) {
            serviceDescriptors
                .AddBuildScripts();
        }
    }
}
