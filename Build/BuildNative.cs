using System.IO.Compression;

using Realynx.CatTail;
using Realynx.CatTail.Attributes;
using Realynx.CatTail.Services.Interfaces;

namespace Build {
    public class BuildNative : IBuildScript {
        private readonly IShell _shell;
        private readonly ISolutionFileReader _solutionFileReader;

        public DirectoryInfo OutputDirectory { get; set; }

        public BuildNative(IShell shell, ISolutionFileReader solutionFileReader) {
            _shell = shell;
            _solutionFileReader = solutionFileReader;
        }

        public void ConfigureBuild(BuildOptions configure) {
            OutputDirectory = configure.outputDirectory;

            configure
                .Stage("Build")
                .Step(nameof(Compile));
        }

        public void Compile() {
            var nativeProjects = _solutionFileReader.GetNativeProjects();
            foreach (var nativeProject in nativeProjects) {
                Console.WriteLine($"Publishing project: {nativeProject}");

                switch (Environment.OSVersion.Platform) {
                    case PlatformID.Win32S:
                    case PlatformID.Win32Windows:
                    case PlatformID.Win32NT:
                    case PlatformID.WinCE:
                        PublishMsBuild(OutputDirectory.FullName, nativeProject);
                        break;
                    case PlatformID.Unix:
                        PublishGcc(OutputDirectory.FullName, nativeProject);
                        break;
                    case PlatformID.MacOSX:
                        break;
                    default:
                        break;
                }

            }
        }

        private void PublishGcc(string buildResultDir, string project) {
            var outputFolder = Path.Combine(buildResultDir, "linux", Path.GetFileNameWithoutExtension(project));

            var buildResult = _shell.Run(
                "bash",
                $"{project}",
                buildResultDir);

            var zipFilePath = $"{outputFolder}.zip";
            if (File.Exists(zipFilePath)) {
                File.Delete(zipFilePath);
            }

            ZipFile.CreateFromDirectory(outputFolder, zipFilePath, CompressionLevel.Optimal, includeBaseDirectory: true);
        }

        private void PublishMsBuild(string buildResultDir, string project) {
            var outputFolder = Path.Combine(buildResultDir, "windows", Path.GetFileNameWithoutExtension(project));
            var msBuild = "C:\\Program Files\\Microsoft Visual Studio\\2022\\Enterprise\\MSBuild\\Current\\Bin\\amd64\\msbuild.exe";

            var buildResult = _shell.Run(
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
}
