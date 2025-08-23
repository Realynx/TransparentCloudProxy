using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

public static class Program
{
    public static void Main(string[] args)
    {
        Console.WriteLine("Running restore");
        Shell.Run("dotnet", "restore");

        var buildResultDir = Directory.CreateDirectory("./BuildResult");
        Console.WriteLine($"Using output directory: {buildResultDir.FullName}");

        var dotnetProjects = GetDotnetProjects(includeTestProjects: false);
        foreach (var dotnetProject in dotnetProjects)
        {
            Console.WriteLine($"Publishing project: {dotnetProject}");

            var outputFolder = Path.Combine(buildResultDir.FullName, Path.GetFileNameWithoutExtension(dotnetProject));
            var buildResult = Shell.Run(
                "dotnet",
                [
                    "publish",
                    "-c", "Release",
                    "-o", outputFolder,
                    dotnetProject
                ]);
            
            Console.WriteLine(buildResult);
        }
        
        Console.WriteLine("Done!");
    }

    private static string[] GetDotnetProjects(bool includeTestProjects)
    {
        var dotnetProjects = Directory.GetFiles(".", "*.csproj", new EnumerationOptions { RecurseSubdirectories = true, MaxRecursionDepth = 1 });

        if (includeTestProjects)
        {
            dotnetProjects = dotnetProjects.Where(i => !i.Contains("Test")).ToArray();
        }

        dotnetProjects = dotnetProjects.Select(i => Path.GetFullPath(i)).ToArray();
        return dotnetProjects;
    }
}

public static class Shell
{
    public static string Run(string program, params string[] arguments)
    {
        try
        {
            var programPath = FindProgramPath(program);
            if (string.IsNullOrWhiteSpace(programPath))
            {
                throw new FileNotFoundException($"Failed to find '{program}'");
            }

            Console.WriteLine($"Running {programPath} with args: {CombineArguments(arguments)}");

            var process = Process.Start(new ProcessStartInfo(program, arguments)
            {
                RedirectStandardOutput = true,
                CreateNoWindow = true,
                UseShellExecute = false
            });

            process.WaitForExit();
            return process.StandardOutput.ReadToEnd();
        }
        catch (Exception e)
        {
            return e.ToString();
        }

        static string CombineArguments(IEnumerable<string> args)
        {
            return string.Join(' ', args.Select(x =>
            {
                if (!x.StartsWith('"') && !x.StartsWith('\'') && x.Contains(' '))
                    return $"\"{x}\"";

                return x;
            }));
        }
    }

    private static string? FindProgramPath(string programName)
    {
        if (OperatingSystem.IsWindows() && !programName.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
        {
            programName += ".exe";
        }

        if (File.Exists(programName))
        {
            return Path.GetFullPath(programName);
        }

        var pathEnv = Environment.GetEnvironmentVariable("PATH");
        if (string.IsNullOrEmpty(pathEnv))
        {
            return null;
        }

        foreach (var pathDir in pathEnv.Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries))
        {
            var potentialPath = Path.Combine(pathDir, programName);
            if (File.Exists(potentialPath))
            {
                return Path.GetFullPath(potentialPath);
            }
        }

        return null;
    }
}
