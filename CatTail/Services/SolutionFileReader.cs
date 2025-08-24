using Realynx.CatTail.Services.Interfaces;

namespace Realynx.CatTail.Services {
    public class SolutionFileReader : ISolutionFileReader {
        private readonly string _solutionFile;

        public SolutionFileReader(string solutionFile) {
            _solutionFile = solutionFile;
        }

        public string[] GetDotNetProjects() {
            return Array.Empty<string>();
        }
    }
}
