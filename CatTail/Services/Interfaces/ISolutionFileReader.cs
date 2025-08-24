namespace Realynx.CatTail.Services.Interfaces {
    public interface ISolutionFileReader {
        string[] GetDotNetProjects();
        string[] GetNativeProjects();
    }
}