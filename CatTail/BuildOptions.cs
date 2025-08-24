
namespace Realynx.CatTail {
    public class BuildOptions {
        public string StageName { get; private set; }
        public List<string> Steps { get; private set; }
        public DirectoryInfo OutputDirectory { get; private set; }

        public BuildOptions WithBuildDef() {
            return this;
        }

        public BuildOptions WithCompillation() {
            return this;
        }

        public BuildOptions WithStageName(string name) {
            StageName = name;
            return this;
        }

        public BuildOptions WithStep(string stepName) {
            Steps.Add(stepName);
            return this;
        }

        public BuildOptions WithOutputDirectory(DirectoryInfo directoryInfo) {
            OutputDirectory = directoryInfo;
            return this;
        }
    }
}
