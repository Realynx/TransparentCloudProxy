using System.Text.Json;

using TransparentCloudServerProxy.Managed.Models;
using TransparentCloudServerProxy.ProxyBackend.Interfaces;
using TransparentCloudServerProxy.ProxyBackend.ManagedProxy;
using TransparentCloudServerProxy.ProxyBackend.NativeCProxy;
using TransparentCloudServerProxy.ProxyBackend.WindowsPF;
using TransparentCloudServerProxy.SystemTools;

namespace TransparentCloudServerProxy.Cli {
    internal class Program {
        static async Task Main(string[] args) {
            IProxyConfig proxyConfig;
            Console.WriteLine("Welcome to Cloud Proxy.");

            if (File.Exists(Path.GetFullPath("appsettings.json"))) {
                var jsonConfig = File.ReadAllText(Path.GetFullPath("appsettings.json"));

                Console.WriteLine(Path.GetFullPath("appsettings.json"));
                Console.WriteLine(jsonConfig);

                proxyConfig = JsonSerializer.Deserialize<ProxyConfig>(jsonConfig);
                if (string.IsNullOrWhiteSpace(proxyConfig.PacketEngine)) {
                    proxyConfig.PacketEngine = "Managed";
                }
            }
            else {
                proxyConfig = new ProxyConfig();
            }

            ResetLowLevelPacketFiltering(proxyConfig);

            var proxies = new List<IProxy>();
            Console.WriteLine($"Adding {proxyConfig.Proxies.Length} proxies from config");

            for (uint x = 0; x < proxyConfig.Proxies.Length; x++) {
                var proxy = proxyConfig.Proxies[x];
                proxy.Id = x;
                Console.WriteLine(proxy.ToString());

                IProxy proxyImplementation;
                switch (proxyConfig.PacketEngine) {
                    case "NetFilter":
                        proxyImplementation = NativeCProxy.FromInstance(proxy);
                        break;
                    case "NativeC":
                        proxyImplementation = NativeCProxy.FromInstance(proxy);
                        break;
                    case "WindowsPF":
                        proxyImplementation = WindowsPFProxy.FromInstance(proxy);
                        break;

                    default:
                        proxyImplementation = ManagedProxy.FromInstance(proxy);
                        break;
                }

                proxies.Add(proxyImplementation);
                proxyImplementation.Start();
            }

            Console.WriteLine($"Proxy is running...");
            Console.WriteLine($"Type 'exit' to turn off forwarding.");

            Console.WriteLine($"\n\r\n\r");
            Console.WriteLine($"Packet Engine: {proxyConfig.PacketEngine}");
            foreach (var endpoint in proxies) {
                Console.WriteLine(endpoint.ToString());
            }

            for (var input = Console.ReadLine(); !input.Equals("exit", StringComparison.OrdinalIgnoreCase); input = Console.ReadLine()) {
            }

            ResetLowLevelPacketFiltering(proxyConfig);
        }

        private static void ResetLowLevelPacketFiltering(IProxyConfig proxyConfig) {
            if (proxyConfig.PacketEngine == "WindowsPF") {
                new Netsh().ResetState();
            }

            if (proxyConfig.PacketEngine == "NetFilter") {
                new NetFilter().ResetTables();
            }
        }
    }
}
