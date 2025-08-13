using System.Text.Json;

using TransparentCloudServerProxy.Cli.Models;
using TransparentCloudServerProxy.Managed;

namespace TransparentCloudServerProxy.Cli {
    internal class Program {
        static async Task Main(string[] args) {
            ProxyConfig proxyConfig = new();
            Console.WriteLine("Welcome to Cloud Proxy.");

            if (File.Exists(Path.GetFullPath("appsettings.json"))) {
                var jsonConfig = File.ReadAllText(Path.GetFullPath("appsettings.json"));

                Console.WriteLine(Path.GetFullPath("appsettings.json"));
                Console.WriteLine(jsonConfig);

                proxyConfig = JsonSerializer.Deserialize<ProxyConfig>(jsonConfig);
            }

            var proxyService = new ManagedProxyService();
            Console.WriteLine($"Adding {proxyConfig.ManagedProxyEntry.Length} proxies from config");

            foreach (var entry in proxyConfig.ManagedProxyEntry) {
                Console.WriteLine(entry.ToString());
                proxyService.AddProxyEntry(entry);
            }

            proxyService.StartAllProxies();

            Console.WriteLine($"Proxy is running...");

            while (true) {
                Console.ReadLine();
            }
        }
    }
}
