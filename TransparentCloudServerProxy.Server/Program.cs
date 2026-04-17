using System.Text.Json;

using TransparentCloudServerProxy.Interfaces;
using TransparentCloudServerProxy.Models;
using TransparentCloudServerProxy.ProxyBackend.Interfaces;
using TransparentCloudServerProxy.Services;

namespace TransparentCloudServerProxy.Server {
    internal class Program {
        static void Main(string[] args) {
            Console.WriteLine("Welcome to Cloud Proxy.");

            var proxyConfig = LoadProxyConfig();
            proxyConfig.Proxies ??= [];

            var proxyFactory = new ProxyFactory();
            var packetFilterResetService = new PacketFilterResetService();
            packetFilterResetService.Reset(proxyConfig.Proxies);

            var proxies = new List<IProxy>();
            Console.WriteLine($"Adding {proxyConfig.Proxies.Length} proxies from config");

            foreach (var proxy in proxyConfig.Proxies) {
                Console.WriteLine(proxy);

                var proxyImplementation = proxyFactory.Create(proxy);
                proxies.Add(proxyImplementation);
                proxyImplementation.Start();
            }

            Console.WriteLine("Proxy is running...");
            Console.WriteLine("Type 'exit' to turn off forwarding.");

            Console.WriteLine("\n\r\n\r");
            foreach (var proxy in proxies) {
                Console.WriteLine($"[{proxy.PacketEngine}] {proxy}");
            }

            while (true) {
                var input = Console.ReadLine();
                if (input is null || input.Equals("exit", StringComparison.OrdinalIgnoreCase)) {
                    break;
                }
            }

            foreach (var proxy in proxies) {
                proxy.Stop();
            }
        }

        private static ProxyConfig LoadProxyConfig() {
            var appSettingsPath = Path.GetFullPath("appsettings.json");
            if (!File.Exists(appSettingsPath)) {
                return new ProxyConfig();
            }

            var jsonConfig = File.ReadAllText(appSettingsPath);
            Console.WriteLine(appSettingsPath);
            Console.WriteLine(jsonConfig);

            return JsonSerializer.Deserialize<ProxyConfig>(jsonConfig) ?? new ProxyConfig();
        }
    }
}
