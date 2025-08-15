using System.Text.Json;

using TransparentCloudServerProxy.Managed.Models;
using TransparentCloudServerProxy.ProxyBackend.Interfaces;
using TransparentCloudServerProxy.ProxyBackend.ManagedProxy;
using TransparentCloudServerProxy.ProxyBackend.NativeCProxy;
using TransparentCloudServerProxy.ProxyBackend.UnixNetfilter;
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


            if (proxyConfig.PacketEngine == "NetFilter") {
                new NetFilter().ResetTables();
            }

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

                    default:
                        proxyImplementation = NativeCProxy.FromInstance(proxy);
                        break;
                }

                proxies.Add(proxyImplementation);
                proxyImplementation.Start();
            }

            Console.WriteLine($"Proxy is running...");
            Console.Clear();

            while (true) {
                Console.CursorTop = 0;
                Console.CursorLeft = 0;

                Console.WriteLine($"Packet Engine: {proxyConfig.PacketEngine}");
                foreach (var endpoint in proxies) {
                    Console.WriteLine(endpoint.ToString());
                }

                Console.WriteLine("\n\r\n\r\n\r\n\r");
                await Task.Delay(TimeSpan.FromSeconds(1));
            }
        }
    }
}
