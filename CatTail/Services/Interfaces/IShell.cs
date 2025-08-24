namespace Realynx.CatTail.Services.Interfaces {
    public interface IShell {
        string Run(string program, params string[] arguments);
        string RunSensitive(string program, params string[] arguments);
    }
}