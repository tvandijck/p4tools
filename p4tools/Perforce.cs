using Spectre.Console;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace p4tools
{
    internal static class Perforce
    {
        public static PerforceChange? GetLatest()
        {
            var exec = new Executor(Directory.GetCurrentDirectory());

            if (exec.ExecuteProcess("p4", "-z tag", "-Mj", "changes -m 1 -l") != 0)
            {
                return null;
            }

            var desc = exec.StdOut.FirstOrDefault();
            if (string.IsNullOrEmpty(desc))
            {
                return null;
            }

            return JsonSerializer.Deserialize<PerforceChange>(desc);
        }

        public static bool Sync(string changeList)
        {
            return AnsiConsole.Status()
                .Spinner(Spinner.Known.Star)
                .SpinnerStyle(Style.Parse("green bold"))
                .Start("Syncing perforce...", ctx =>
            {
                var exec = new Executor(Directory.GetCurrentDirectory(), true);
                return (exec.ExecuteProcess("p4", $"sync ...@{changeList}") == 0);
            });
        }

        public static void Delete(string filename)
        {
            var exec = new Executor(Directory.GetCurrentDirectory());
            exec.ExecuteProcess("p4", "delete", Quoted(filename));
        }

        public static string[] Print(string depotFile)
        {
            var exec = new Executor(Directory.GetCurrentDirectory());

            if (exec.ExecuteProcess("p4", "print", "-q", Quoted(depotFile)) != 0)
            {
                return Array.Empty<string>();
            }

            return exec.StdOut.ToArray();
        }

        public static void Checkout(string filename)
        {
            var exec = new Executor(Directory.GetCurrentDirectory());
            exec.ExecuteProcess("p4", "edit", Quoted(filename));
        }

        public static PerforceChangeList? GetChange(string changeList)
        {
            var exec = new Executor(Directory.GetCurrentDirectory());

            if (exec.ExecuteProcess("p4", "-z tag", "-Mj", "change -o", changeList) != 0)
            {
                return null;
            }

            var desc = exec.StdOut.FirstOrDefault();
            if (string.IsNullOrEmpty(desc))
            {
                return null;
            }

            var output = new PerforceChangeList();

            var root = JsonNode.Parse(desc);
            if (root == null)
            {
                return null;
            }

            output.Change = root["Change"]?.ToString();
            output.Client = root["Client"]?.ToString();
            output.Date = root["Date"]?.ToString();
            output.Description = root["Description"]?.ToString();
            output.Status = root["Status"]?.ToString();
            output.Type = root["Type"]?.ToString();
            output.User = root["User"]?.ToString();

            int idx = 0;
            for (; ; )
            {
                var file = root[$"Files{idx++}"];
                if (file == null)
                    break;
                output.Files.Add(file.ToString());
            }

            return output;
        }

        public static void Add(string filename)
        {
            var exec = new Executor(Directory.GetCurrentDirectory());
            exec.ExecuteProcess("p4", "add", Quoted(filename));
        }

        public static string[] GetPatch(string depotFile)
        {
            var exec = new Executor(Directory.GetCurrentDirectory());

            if (exec.ExecuteProcess("p4", "diff", "-dup", Quoted(depotFile)) != 0)
            {
                return Array.Empty<string>();
            }

            return exec.StdOut.ToArray();
        }

        private static string Quoted(string value)
        {
            return '"' + value + '"';
        }
    }
}
