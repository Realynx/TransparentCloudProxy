#:package Microsoft.Build.Locator@1.9.1

using System.Diagnostics;
using System.IO.Compression;
using System.Runtime.InteropServices;
using System.Text;

using Microsoft.Build.Locator;

namespace BuildAgent.Script {
    internal class Program {
        public static void Main(string[] args) {
            Environment.CurrentDirectory = "C:\\Users\\poofi\\source\\repos\\TransparentCloudProxy";
            var buildResultDir = Directory.CreateDirectory("./BuildResult");
            Console.WriteLine($"Using output directory: {buildResultDir.FullName}");

            if (Directory.Exists(buildResultDir.FullName)) {
                Directory.Delete(buildResultDir.FullName, true);
            }

            DotNetPublisher.Publish(buildResultDir.FullName);
            NativePublisher.Publish(buildResultDir.FullName);

            Console.WriteLine("Done!");
        }
    }

    public static class NativePublisher {
        public static void Publish(string buildResultDir) {
            var nativeProjects = GetNativeProjects(includeTestProjects: false);
            foreach (var nativeProject in nativeProjects) {
                Console.WriteLine($"Publishing project: {nativeProject}");

                switch (Environment.OSVersion.Platform) {
                    case PlatformID.Win32S:
                    case PlatformID.Win32Windows:
                    case PlatformID.Win32NT:
                    case PlatformID.WinCE:
                        MSBuildPublisher.Publish(buildResultDir, nativeProject);
                        break;
                    case PlatformID.Unix:
                        GccPublisher.Publish(buildResultDir, nativeProject);
                        break;
                    case PlatformID.MacOSX:
                        break;
                    default:
                        break;
                }
            }
        }

        private static string[] GetNativeProjects(bool includeTestProjects) {
            var dotnetProjects = Directory.GetFiles(".", "*.vcxproj", new EnumerationOptions { RecurseSubdirectories = true, MaxRecursionDepth = 1 });

            if (!includeTestProjects) {
                dotnetProjects = dotnetProjects.Where(i => !i.Contains("Test")).ToArray();
            }

            dotnetProjects = dotnetProjects.Select(i => Path.GetFullPath(i)).ToArray();

            return dotnetProjects;
        }
    }

    public static class GccPublisher {
        public static void Publish(string buildResultDir, string project) {
            var outputFolder = Path.Combine(buildResultDir, "linux", Path.GetFileNameWithoutExtension(project));

            var buildResult = Shell.Run(
                "bash",
                $"{project}",
                buildResultDir);

            var zipFilePath = $"{outputFolder}.zip";
            if (File.Exists(zipFilePath)) {
                File.Delete(zipFilePath);
            }

            ZipFile.CreateFromDirectory(outputFolder, zipFilePath, CompressionLevel.Optimal, includeBaseDirectory: true);
        }
    }

    public static class MSBuildPublisher {
        public static void Publish(string buildResultDir, string project) {
            var outputFolder = Path.Combine(buildResultDir, "windows", Path.GetFileNameWithoutExtension(project));
            var mostRecentVsStudio = MSBuildLocator.QueryVisualStudioInstances().OrderByDescending(instance => instance.Version).First();

            var msBuild = "C:\\Program Files\\Microsoft Visual Studio\\2022\\Enterprise\\MSBuild\\Current\\Bin\\amd64\\msbuild.exe";

            var buildResult = Shell.Run(
                msBuild,
                project,
                "/t:Build",
                "/p:Configuration=Release",
                "/p:Platform=x64",
                $"/p:OutDir={outputFolder}");

            var zipFilePath = $"{outputFolder}.zip";
            if (File.Exists(zipFilePath)) {
                File.Delete(zipFilePath);
            }

            ZipFile.CreateFromDirectory(outputFolder, zipFilePath, CompressionLevel.Optimal, includeBaseDirectory: true);
        }
    }

    public static class DotNetPublisher {
        public static void Publish(string buildResultDir) {
            Console.WriteLine("Running restore");
            Shell.Run("dotnet", "restore");

            var dotnetProjects = GetDotnetProjects(includeTestProjects: false);
            foreach (var dotnetProject in dotnetProjects) {
                Console.WriteLine($"Publishing project: {dotnetProject}");

                PublishPlatform(OSPlatform.Windows, buildResultDir, dotnetProject);
                PublishPlatform(OSPlatform.Linux, buildResultDir, dotnetProject);
            }
        }

        private static void PublishPlatform(OSPlatform platform, string buildResultDir, string dotnetProject) {
            var outputFolder = Path.Combine(buildResultDir, platform.ToString().ToLower(), Path.GetFileNameWithoutExtension(dotnetProject));
            var runtimeIdentifier = platform == OSPlatform.Windows ? "win-x64" : "linux-x64";

            var buildResult = Shell.Run(
                "dotnet",
                "publish",
                "-c", "Release",
                "-o", outputFolder,
                "-p:ExcludeCNativeCompile=true",
                "-r", runtimeIdentifier,
                dotnetProject);

            var zipFilePath = $"{outputFolder}.zip";
            if (File.Exists(zipFilePath)) {
                File.Delete(zipFilePath);
            }

            ZipFile.CreateFromDirectory(outputFolder, zipFilePath, CompressionLevel.Optimal, includeBaseDirectory: true);
        }

        private static string[] GetDotnetProjects(bool includeTestProjects) {
            var dotnetProjects = Directory.GetFiles(".", "*.csproj", new EnumerationOptions { RecurseSubdirectories = true, MaxRecursionDepth = 1 });

            if (!includeTestProjects) {
                dotnetProjects = dotnetProjects.Where(i => !i.Contains("Test")).ToArray();
            }

            dotnetProjects = dotnetProjects.Select(i => Path.GetFullPath(i)).ToArray();

            return dotnetProjects;
        }
    }

    public static class Shell {
        public static string Run(string program, params string[] arguments) => RunInternal(program, arguments, false);

        public static string RunSensitive(string program, params string[] arguments) => RunInternal(program, arguments, true);

        private static string RunInternal(string program, string[] arguments, bool sensitive) {
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

        private static string? FindProgramPath(string programName) {
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