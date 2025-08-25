
using System.IO.Compression;

using Realynx.CatTail;
using Realynx.CatTail.Attributes;
using Realynx.CatTail.Services.Interfaces;

namespace Build {
    internal class BuildDotNet : IBuildScript {
        private readonly IShell _shell;
        private readonly ISolutionFileReader _solutionFileReader;
        private DirectoryInfo _outputDirectory;

        public BuildDotNet(IShell shell, ISolutionFileReader solutionFileReader) {
            _shell = shell;
            _solutionFileReader = solutionFileReader;
        }

        public void ConfigureBuild(BuildOptions configure) {
            // Configuration concept:
            // configure
            //     .Name("Build .NET")
            //     .Stage("Build")
            //     .Description("This builds all the .NET projects")
            //     .DependsOn(typeof(OtherJob), typeof(AnotherJob))
            //     .RequireWindows(onFailure: Skip|Fail) // Nice to have
            //     .Step("Step 1", nameof(Step1))
            //     .CommandStep("Step 2", "echo \"Hello, world!\""); // <--- This is inlined into the yaml, replacing the C# call
            //
            // Alternative step concept:
            //     .Step("Step 1", Dotnet.Install.Sdk("9.0.x"))
            //     .Step("Step 2", nameof(Step1))
            //     .Step("Step 3", Shell("echo \"Hello, world!\""))
            _outputDirectory = configure.outputDirectory;

            configure
                .Stage("Build")
                .Step(nameof(CompileWindows));
        }

        public void CompileWindows() {
            var dotnetProjects = _solutionFileReader.GetDotNetProjects();
            foreach (var project in dotnetProjects) {
                PublishPlatform("windows", _outputDirectory.FullName, project);
            }
        }

        private void PublishPlatform(string platform, string buildResultDir, string dotnetProject) {
            var outputFolder = Path.Combine(buildResultDir, platform, Path.GetFileNameWithoutExtension(dotnetProject));
            var rArgument = platform == "windows" ? "win-x64" : "linux-x64";

            var buildResult = _shell.Run(
                    "dotnet",
                    "publish",
                    "-c", "Release",
                    "-o", outputFolder,
                    "-p:ExcludeCNativeCompile=true",
                    "-r", rArgument,
                    dotnetProject);

            var zipFilePath = $"{outputFolder}.zip";
            if (File.Exists(zipFilePath)) {
                File.Delete(zipFilePath);
            }

            ZipFile.CreateFromDirectory(outputFolder, zipFilePath, CompressionLevel.Optimal, includeBaseDirectory: true);
        }
    }
}
