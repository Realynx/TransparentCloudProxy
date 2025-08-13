using System.Text.Json;

using TransparentCloudServerProxy.Cli.Models;
using TransparentCloudServerProxy.Managed.Interfaces;
using TransparentCloudServerProxy.Managed.ManagedCode;
using TransparentCloudServerProxy.Managed.NativeC;

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
                if (string.IsNullOrWhiteSpace(proxyConfig.PacketEngine)) {
                    proxyConfig.PacketEngine = "Managed";
                }
            }

            var proxyEndpoints = new List<IProxyEndpoint>();

            Console.WriteLine($"Adding {proxyConfig.ManagedProxyEntry.Length} proxies from config");
            foreach (var entry in proxyConfig.ManagedProxyEntry) {
                Console.WriteLine(entry.ToString());

                switch (proxyConfig.PacketEngine) {
                    case "NativeC":
                        var nativeProxyEndpoint = new NativeCProxyEndpoint(entry);
                        proxyEndpoints.Add(nativeProxyEndpoint);
                        nativeProxyEndpoint.Start();
                        break;
                    case "NativeR":

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
                    Console.WriteLine($"[{endpoint.GetAverageDelayNanoSecond() / 1000000:F4} Ms] {endpoint}");
                }

                Console.WriteLine("\n\r\n\r\n\r\n\r");
                await Task.Delay(TimeSpan.FromSeconds(1));
            }
        }
    }
}
