using System.Diagnostics;

using TransparentCloudServerProxy.Managed.Interfaces;

namespace TransparentCloudServerProxy.SystemTools {
    public class Netsh : ISystemProgram {
        public string RunCommand(string arguments) {
            try {
                var netFilterProcess = Process.Start(new ProcessStartInfo("netsh", arguments) {
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

        public void ResetState() {
            RunCommand("interface portproxy reset");
        }
    }
}
