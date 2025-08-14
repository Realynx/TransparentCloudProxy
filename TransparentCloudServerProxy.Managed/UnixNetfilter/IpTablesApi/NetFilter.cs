using System.Diagnostics;

namespace TransparentCloudServerProxy.Managed.UnixNetfilter.IpTablesApi {
    public class NetFilter {
        public NetFilter() {

        }

        public static string RunNetFilterCommand(string arguments) {
            try {
                var netFilterProcess = Process.Start(new ProcessStartInfo("nft", arguments) {
                    RedirectStandardOutput = true,
                    UseShellExecute = false
                });

                netFilterProcess.WaitForExit();
                return netFilterProcess.StandardOutput.ReadToEnd();
            }
            catch (Exception e) {
                return e.ToString();
            }
        }

        public static void ResetTables() {
            RunNetFilterCommand("flush ruleset");

            Console.WriteLine(RunNetFilterCommand($"add table ip proxy"));
            Console.WriteLine(RunNetFilterCommand("add chain ip proxy prerouting { type nat hook prerouting priority -100; }"));
            Console.WriteLine(RunNetFilterCommand("add chain ip proxy postrouting { type nat hook postrouting priority 100; }"));
            Console.WriteLine(RunNetFilterCommand("add rule ip proxy postrouting oifname != \"lo\" masquerade"));
        }
    }
}
