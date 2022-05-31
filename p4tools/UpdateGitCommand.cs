using Spectre.Console;
using Spectre.Console.Cli;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

namespace p4tools
{
    internal class UpdateGitCommand : Command<UpdateGitCommand.Settings>
    {
        public class Settings : CommandSettings
        {
        }

        public class State
        {
            public string? CL { get;set; }
        }

        public override int Execute([NotNull] CommandContext context, [NotNull] Settings settings)
        {
            const string stateFile = ".state.json";
            
            State currentState = new State();
            if (File.Exists(stateFile))
            { 
                var state = JsonSerializer.Deserialize<State>(File.ReadAllText(stateFile));
                currentState = state ?? currentState;
            }

            var change = Perforce.GetLatest();
            if (change == null || string.IsNullOrEmpty(change.CL))
            {
                AnsiConsole.WriteLine("ERROR: failed to get deserialize change description");
                return -1;
            }

            AnsiConsole.WriteLine($"Latest = {change.CL}");
            if (currentState.CL == change.CL)
            {
                AnsiConsole.WriteLine("No updates found");
                return 0;
            }

            // sync perforce.
            if (!Perforce.Sync(change.CL))
            {
                AnsiConsole.WriteLine($"ERROR: failed to sync to CL{change.CL}");
                return -1;
            }

            // update state.
            currentState.CL = change.CL;
            File.WriteAllText(stateFile, JsonSerializer.Serialize(currentState));

            // 
            Git.AddAll();
            Git.Commit($"Perforce @ {change.CL}\n{change.Description}");

            return 0;
        }
    }
}