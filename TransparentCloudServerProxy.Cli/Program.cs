using Newtonsoft.Json;

using TransparentCloudServerProxy.Cli.Models;
using TransparentCloudServerProxy.Managed;

namespace TransparentCloudServerProxy.Cli {
    internal class Program {
        static async Task Main(string[] args) {
            ProxyConfig proxyConfig = new();
            await Console.Out.WriteLineAsync("Welcome to Cloud Proxy.");

            if (File.Exists("appsettings.json")) {
                var appSettings = JsonConvert.DeserializeObject<ProxyConfig>(File.ReadAllText("appsettings.json"));
                if (appSettings is not null) {
                    proxyConfig = appSettings;
                }
            }

            var proxyService = new ManagedProxyService();
            foreach (var entry in proxyConfig.ManagedProxyEntry) {
                await AddEntry(proxyService, entry);
            }

            proxyService.StartAllProxies();

            await Console.Out.WriteLineAsync($"Proxy is running...");
            while (true) {
                Console.ReadLine();
            }
        }

        private static async Task AddEntry(ManagedProxyService proxyService, ManagedProxyEntry managedProxyEntry) {
            await Console.Out.WriteLineAsync(managedProxyEntry.ToString());
            proxyService.AddProxyEntry(managedProxyEntry);
        }
    }
}
