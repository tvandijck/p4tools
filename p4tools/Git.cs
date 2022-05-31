using Spectre.Console;

namespace p4tools
{
    internal class Git
    {
        internal static bool AddAll()
        {
            return AnsiConsole.Status()
                .Spinner(Spinner.Known.Star)
                .SpinnerStyle(Style.Parse("green bold"))
                .Start("Git add...", ctx =>
                {
                    var exec = new Executor(Directory.GetCurrentDirectory(), true);
                    return exec.ExecuteProcess("git", "add .") == 0;
                });
        }

        internal static bool Commit(string message)
        {
            return AnsiConsole.Status()
                .Spinner(Spinner.Known.Star)
                .SpinnerStyle(Style.Parse("green bold"))
                .Start("Git commit...", ctx =>
                {
                    var exec = new Executor(Directory.GetCurrentDirectory(), true);
                    return exec.ExecuteProcess("git", "commit", "-m", $"\"{message}\"") == 0;
                });
        }
    }
}