
namespace Realynx.CatTail {
    public class BuildOptions {
        internal List<string> steps;
        public string stageName;
        public DirectoryInfo outputDirectory;


        public BuildOptions StageName(string name) {
            stageName = name;
            return this;
        }

        public BuildOptions Step(string stepName) {
            steps.Add(stepName);
            return this;
        }

        public BuildOptions OutputDirectory(DirectoryInfo directoryInfo) {
            outputDirectory = directoryInfo;
            return this;
        }
    }
}
