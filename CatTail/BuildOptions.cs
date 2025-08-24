
namespace Realynx.CatTail {
    public class BuildOptions {
        public string StageName { get; set; }
        public List<string> Steps { get; set; }

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
    }
}
