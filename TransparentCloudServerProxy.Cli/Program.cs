using System.Text.Json;

using TransparentCloudServerProxy.Managed.Models;
using TransparentCloudServerProxy.ProxyBackend.Interfaces;
using TransparentCloudServerProxy.ProxyBackend.Managed;
using TransparentCloudServerProxy.ProxyBackend.NativeC;
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
                switch (proxy.PacketEngine) {
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
                        proxy.PacketEngine = "Managed";
                        proxyImplementation = ManagedProxy.FromInstance(proxy);
                        break;
                }

                proxies.Add(proxyImplementation);
                proxyImplementation.Start();
            }

            Console.WriteLine($"Proxy is running...");
            Console.WriteLine($"Type 'exit' to turn off forwarding.");

            Console.WriteLine($"\n\r\n\r");
            foreach (var proxy in proxies) {
                Console.WriteLine($"[{proxy.PacketEngine}] {proxy}");
            }

            for (var input = Console.ReadLine(); !input.Equals("exit", StringComparison.OrdinalIgnoreCase); input = Console.ReadLine()) {
            }

            ResetLowLevelPacketFiltering(proxyConfig);
        }

        private static void ResetLowLevelPacketFiltering(IProxyConfig proxyConfig) {
            if (proxyConfig.Proxies.Any(i => i.PacketEngine == "WindowsPF")) {
                new Netsh().ResetState();
            }

            if (proxyConfig.Proxies.Any(i => i.PacketEngine == "NetFilter")) {
                new NetFilter().ResetTables();
            }
        }
    }
}
