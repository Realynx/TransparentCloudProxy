
using System.IO.Compression;

using Realynx.CatTail.Attributes;
using Realynx.CatTail.Services;
using Realynx.CatTail.Services.Interfaces;

namespace Build {
    internal class BuildDotNet : IBuildScript {
        private readonly IShell _shell;
        private readonly ISolutionFileReader _solutionFileReader;
        private CancellationToken _cancellationToken;

        public BuildDotNet(IShell shell, ISolutionFileReader solutionFileReader) {
            _shell = shell;
            _solutionFileReader = solutionFileReader;
        }

        public void ConfigureBuild(object configure) {

        }

        public Task StartAsync(CancellationToken cancellationToken, DirectoryInfo outputDirectory) {
            _cancellationToken = cancellationToken;

            var dotnetProjects = _solutionFileReader.GetDotNetProjects();
            foreach (var project in dotnetProjects) {
                PublishPlatform("windows", );
            }
        }

        public Task StopAsync(CancellationToken cancellationToken) {

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
