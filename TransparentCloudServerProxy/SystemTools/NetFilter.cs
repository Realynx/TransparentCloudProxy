using System.Diagnostics;

using TransparentCloudServerProxy.Interfaces;

namespace TransparentCloudServerProxy.SystemTools {
    public class NetFilter : ISystemProgram {
        public NetFilter() {

        }

        public string RunCommand(string arguments) {
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

        public void ResetTables() {
            RunCommand("flush ruleset");

            RunCommand($"add table ip proxy");
            RunCommand("add chain ip proxy prerouting { type nat hook prerouting priority -100; }");
            RunCommand("add chain ip proxy postrouting { type nat hook postrouting priority 100; }");
            RunCommand("add rule ip proxy postrouting oifname != \"lo\" masquerade");
        }
    }
}
