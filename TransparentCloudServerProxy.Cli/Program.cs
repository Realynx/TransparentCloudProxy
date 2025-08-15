using System.Text.Json;

using TransparentCloudServerProxy.Managed.Interfaces;
using TransparentCloudServerProxy.Managed.ManagedCode;
using TransparentCloudServerProxy.Managed.Models;
using TransparentCloudServerProxy.Managed.NativeC;
using TransparentCloudServerProxy.Managed.UnixNetfilter;
using TransparentCloudServerProxy.Managed.UnixNetfilter.IpTablesApi;

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

            var proxyEndpoints = new List<IProxyEndpoint>();

            if (proxyConfig.PacketEngine == "NetFilter") {
                new NetFilter().ResetTables();
            }

            Console.WriteLine($"Adding {proxyConfig.ManagedProxyEntry.Length} proxies from config");
            foreach (var entry in proxyConfig.ManagedProxyEntry) {
                Console.WriteLine(entry.ToString());
                entry.Id = Guid.NewGuid();

                switch (proxyConfig.PacketEngine) {
                    case "NetFilter":
                        var netFilterEndpoint = new NetFilterProxyEndpoint(entry, new NetFilter());
                        proxyEndpoints.Add(netFilterEndpoint);
                        netFilterEndpoint.Start();
                        break;
                    case "NativeC":
                        var nativeProxyEndpoint = new NativeCProxyEndpoint(entry);
                        proxyEndpoints.Add(nativeProxyEndpoint);
                        nativeProxyEndpoint.Start();
                        break;
                    default:
                        var managedProxyEndpoint = new ManagedProxyEndpoint(entry);
                        proxyEndpoints.Add(managedProxyEndpoint);
                        managedProxyEndpoint.Start();
                        break;
                }
            }

            Console.WriteLine($"Proxy is running...");
            Console.Clear();

            while (true) {
                Console.CursorTop = 0;
                Console.CursorLeft = 0;

                Console.WriteLine($"Packet Engine: {proxyConfig.PacketEngine}");
                foreach (var endpoint in proxyEndpoints) {
                    var delayString = $"[{endpoint.GetAverageDelayNanoSecond() / 1000000:F4} Ms] ";

                    if (proxyConfig.PacketEngine == "NetFilter") {
                        delayString = string.Empty;
                    }

                    Console.WriteLine($"{delayString}{endpoint}");
                }

                Console.WriteLine("\n\r\n\r\n\r\n\r");
                await Task.Delay(TimeSpan.FromSeconds(1));
            }
        }
    }
}
