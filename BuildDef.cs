#:package Microsoft.Build.Locator@1.11.2
#:package Spectre.Console@0.54.0

using System.Diagnostics;
using System.IO.Compression;
using System.Runtime.InteropServices;
using System.Text;

using Microsoft.Build.Locator;

using Spectre.Console;

namespace BuildAgent.Script {
    internal class Program {
        public static void Main(string[] args) {
            Environment.CurrentDirectory = Path.GetDirectoryName((string)AppContext.GetData("EntryPointFilePath")!)!;
            var buildResultDir = new DirectoryInfo("./BuildResult");
            AnsiConsole.MarkupLineInterpolated($"[Aqua]Using output directory:[/] [Orange1]{buildResultDir.FullName}[/]");

            if (buildResultDir.Exists) {
				buildResultDir.Delete(recursive: true);
            }
			buildResultDir.Create();
			buildResultDir.Refresh();

            DotNetPublisher.PublishProjects(buildResultDir);
            NativePublisher.PublishProjects(buildResultDir);

            AnsiConsole.MarkupLine("[Green]Build complete![/]");
        }
    }

    public abstract class Publisher {
        protected string? outputFolder;

        public void ZipArtifacts() {
            _ = outputFolder ?? throw new InvalidOperationException($"{nameof(outputFolder)} must be set in Publish() method.");

            if (!Directory.Exists(outputFolder)) {
                AnsiConsole.MarkupLineInterpolated($"[bold Red]Cannot zip, {outputFolder} does not exist![/]");
                return;
            }

            var zipFilePath = $"{outputFolder}.zip";
            if (File.Exists(zipFilePath)) {
                File.Delete(zipFilePath);
            }

            ZipFile.CreateFromDirectory(outputFolder, zipFilePath, CompressionLevel.SmallestSize, includeBaseDirectory: false);
        }
    }

    public static class NativePublisher {
        public static void PublishProjects(DirectoryInfo buildResultDir) {
            var nativeProjects = GetNativeProjects(includeTestProjects: false);
            foreach (var nativeProject in nativeProjects) {
                AnsiConsole.MarkupLineInterpolated($"[Aqua]Publishing project:[/] [Orange1]{nativeProject}[/]");

                switch (Environment.OSVersion.Platform) {
                    case PlatformID.Win32S:
                    case PlatformID.Win32Windows:
                    case PlatformID.Win32NT:
                    case PlatformID.WinCE:
                        new MSBuildPublisher()
                            .Publish(buildResultDir, nativeProject)
                            .ZipArtifacts();
                        break;
                    case PlatformID.Unix:
                        new GccPublisher()
                            .Publish(buildResultDir, nativeProject)
                            .ZipArtifacts();
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
                dotnetProjects = dotnetProjects.Where(x => !x.Contains("Test")).ToArray();
            }

            dotnetProjects = dotnetProjects.Select(Path.GetFullPath).ToArray();

            return dotnetProjects;
        }
    }

    public class GccPublisher : Publisher {
        public Publisher Publish(DirectoryInfo buildResultDir, string project) {
            outputFolder = Path.Combine(buildResultDir.FullName, "linux", Path.GetFileNameWithoutExtension(project));

            Shell.Run(
                "bash",
                $"{project}",
                buildResultDir.FullName);

            return this;
        }
    }

    public class MSBuildPublisher : Publisher {
        public Publisher Publish(DirectoryInfo buildResultDir, string project) {
            outputFolder = Path.Combine(buildResultDir.FullName, "windows", Path.GetFileNameWithoutExtension(project));
            var mostRecentVsStudio = MSBuildLocator.QueryVisualStudioInstances().OrderByDescending(instance => instance.Version).First();

			const string VS2026_BASE_PATH = @"C:\Program Files\Microsoft Visual Studio\18";
			const string VS2026_MSBUILD_PATH = @"MSBuild\Current\Bin\amd64\MSBuild.exe";
			var vsEdition = Directory.GetDirectories(VS2026_BASE_PATH)[0];
            var msBuild = Path.Combine(VS2026_BASE_PATH, vsEdition, VS2026_MSBUILD_PATH);

            var buildResult = Shell.Run(
                msBuild,
                project,
                "/t:Build",
                "/p:Configuration=Release",
                "/p:Platform=x64",
                $"/p:OutDir={outputFolder}");
            
            return this;
        }
    }

    public class DotNetPublisher : Publisher {
        public static void PublishProjects(DirectoryInfo buildResultDir) {
            AnsiConsole.MarkupLine("[Aqua]Restoring .NET projects[/]");
            Shell.Run("dotnet", "restore");

            var dotnetProjects = GetDotnetProjects(includeTestProjects: false);
            foreach (var dotnetProject in dotnetProjects) {
                AnsiConsole.MarkupLineInterpolated($"[Aqua]Publishing project:[/] [Orange1]{dotnetProject}[/]");

                new DotNetPublisher()
                    .PublishPlatform(OSPlatform.Windows, buildResultDir, dotnetProject)
                    .ZipArtifacts();
                new DotNetPublisher()
                    .PublishPlatform(OSPlatform.Linux, buildResultDir, dotnetProject)
                    .ZipArtifacts();
            }
        }

        private static string[] GetDotnetProjects(bool includeTestProjects) {
            var dotnetProjects = Directory.GetFiles(".", "*.csproj", new EnumerationOptions { RecurseSubdirectories = true, MaxRecursionDepth = 1 });

            if (!includeTestProjects) {
                dotnetProjects = dotnetProjects.Where(x => !x.Contains("Test")).ToArray();
            }

            dotnetProjects = dotnetProjects.Select(Path.GetFullPath).ToArray();

            return dotnetProjects;
        }

        private Publisher PublishPlatform(OSPlatform platform, DirectoryInfo buildResultDir, string dotnetProject) {
            outputFolder = Path.Combine(buildResultDir.FullName, platform.ToString().ToLower(), Path.GetFileNameWithoutExtension(dotnetProject));
            var runtimeIdentifier = platform == OSPlatform.Windows ? "win-x64" : "linux-x64";

            Shell.Run(
                "dotnet",
                "publish",
                "-c", "Release",
                "-o", outputFolder,
                "-p:ExcludeCNativeCompile=true",
                "-r", runtimeIdentifier,
                dotnetProject);

            return this;
        }
    }

    public static class Shell {
        /// <summary>Runs a program with the specified arguments.</summary>
        /// <returns>The standard output from the started process.</returns>
        public static string Run(string program, params string[] arguments) => RunInternal(program, arguments, false);

        /// <summary>Runs a program with the specified arguments.</summary>
        /// <returns>The standard output from the started process.</returns>
        /// <remarks>Does not log the arguments to the console.</remarks>
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
                    AnsiConsole.MarkupLineInterpolated($"[Aqua]Running[/] [Orange1]{programPath}[/] [Aqua]with args:[/] [Orange1]{CombineArguments(arguments)}[/]");
                }
                else {
                    AnsiConsole.MarkupLineInterpolated($"[Aqua]Running[/] [Orange1]{programPath}[/] [Aqua]with[/] [Orange1]sensitive args[/]");
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