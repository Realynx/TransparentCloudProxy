using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

public static class Program {
    public static void Main(string[] args) {
        Console.WriteLine("Running restore");
        Shell.Run("dotnet", "restore");

        var buildResultDir = Directory.CreateDirectory("./BuildResult");
        Console.WriteLine($"Using output directory: {buildResultDir.FullName}");

        var dotnetProjects = GetDotnetProjects(includeTestProjects: false);
        foreach (var dotnetProject in dotnetProjects) {
            Console.WriteLine($"Publishing project: {dotnetProject}");

            var outputFolder = Path.Combine(buildResultDir.FullName, Path.GetFileNameWithoutExtension(dotnetProject));
            var buildResult = Shell.Run(
                    "dotnet",
                    "publish",
                    "-c", "Release",
                    "-o", outputFolder,
                    "-p:ExcludeCNativeCompile=true",
                    dotnetProject
                );
        }

        Console.WriteLine("Done!");
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
            var programPath = FindProgramPath(program);
            if (string.IsNullOrWhiteSpace(programPath)) {
                throw new FileNotFoundException($"Failed to find '{program}'");
            }

            if (!sensitive)
                Console.WriteLine($"Running {programPath} with args: {CombineArguments(arguments)}");
            else
                Console.WriteLine($"Running {programPath} with sensitive args");

            var outputBuilder = new StringBuilder();
            var process = Process.Start(new ProcessStartInfo(program, arguments) {
                RedirectStandardOutput = true,
                CreateNoWindow = true,
                UseShellExecute = false
            });

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
                if (!x.StartsWith('"') && !x.StartsWith('\'') && x.Contains(' ')) {
                    return $"\"{x}\"";
                }

                return x;
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
