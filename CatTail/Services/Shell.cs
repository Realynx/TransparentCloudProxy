using System.Diagnostics;
using System.Text;

namespace Realynx.CatTail.Services {
    public class Shell : IShell {
        public string Run(string program, params string[] arguments) => RunInternal(program, arguments, false);

        public string RunSensitive(string program, params string[] arguments) => RunInternal(program, arguments, true);

        private string RunInternal(string program, string[] arguments, bool sensitive) {
            try {
                var programPath = program;
                if (!Path.IsPathFullyQualified(program)) {
                    programPath = FindProgramPath(program);
                }

                if (string.IsNullOrWhiteSpace(programPath)) {
                    throw new FileNotFoundException($"Failed to find '{program}'");
                }

                if (!sensitive) {
                    Console.WriteLine($"Running {programPath} with args: {CombineArguments(arguments)}");
                }
                else {
                    Console.WriteLine($"Running {programPath} with sensitive args");
                }

                var outputBuilder = new StringBuilder();
                var process = Process.Start(new ProcessStartInfo(program, arguments) {
                    RedirectStandardOutput = true,
                    CreateNoWindow = true,
                    UseShellExecute = false
                }) ?? throw new Exception("Could not start process");

                process.OutputDataReceived += (s, e) => {
                    if (e.Data != null) {
                        var message = e.Data.TrimEnd();

                        outputBuilder.AppendLine(message);
                        Console.WriteLine(message);
                    }
                };

                process.BeginOutputReadLine();

                process.WaitForExit();

                return outputBuilder.ToString();
            }
            catch (Exception e) {
                return e.ToString();
            }

            static string CombineArguments(IEnumerable<string> args) {
                return string.Join(' ', args.Select(x => {
                    return !x.StartsWith('"') && !x.StartsWith('\'') && x.Contains(' ') ? $"\"{x}\"" : x;
                }));
            }
        }

        private string? FindProgramPath(string programName) {
            if (OperatingSystem.IsWindows() && !programName.EndsWith(".exe", StringComparison.OrdinalIgnoreCase)) {
                programName += ".exe";
            }

            if (File.Exists(programName)) {
                return Path.GetFullPath(programName);
            }

            var pathEnv = Environment.GetEnvironmentVariable("PATH");
            if (string.IsNullOrEmpty(pathEnv)) {
                return null;
            }

            foreach (var pathDir in pathEnv.Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries)) {
                var potentialPath = Path.Combine(pathDir, programName);
                if (File.Exists(potentialPath)) {
                    return Path.GetFullPath(potentialPath);
                }
            }

            return null;
        }
    }
}
