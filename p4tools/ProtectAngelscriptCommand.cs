using Spectre.Console;
using Spectre.Console.Cli;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

namespace p4tools
{

    internal class ProtectAngelscriptCommand : Command<ProtectAngelscriptCommand.Settings>
    {
        public class Settings : CommandSettings
        {
            [CommandOption("-c|--cl")]
            public string? CL { get; set; }

            [CommandOption("-d|--define")]
            public string? Define { get; set; }

            public override ValidationResult Validate()
            {
                if (string.IsNullOrEmpty(CL))
                {
                    return ValidationResult.Error("Must set CL [-c|--cl]");
                }
                if (string.IsNullOrEmpty(Define))
                {
                    return ValidationResult.Error("Must set Define [-d|--define]");
                }
                return base.Validate();
            }
        }

        public override int Execute([NotNull] CommandContext context, [NotNull] Settings settings)
        {
            var change = Perforce.GetChange(settings.CL!);
            if (change == null)
            {
                AnsiConsole.WriteLine("ERROR: failed to get changelist description");
                return -1;
            }

            foreach (var file in change.Files)
            {
                var ext = Path.GetExtension(file).ToLower();
                if (ext == ".as")
                {
                    AnsiConsole.WriteLine(file);
                    PatchFile(file, settings.Define!);
                }
            }

            return 0;
        }

        private int PatchFile(string depotFile, string define)
        {
            var content = Perforce.Print(depotFile);
            var patch = Perforce.GetPatch(depotFile);

            if (!patch[0].StartsWith("---"))
            {
                AnsiConsole.WriteLine("ERROR: patch output not right");
                return -1;
            }

            if (!patch[1].StartsWith("+++"))
            {
                AnsiConsole.WriteLine("ERROR: patch output not right");
                return -1;
            }

            var outputFile = patch[1].Substring(4);
            var idx = outputFile.IndexOf('\t');
            if (idx < 0)
            {
                AnsiConsole.WriteLine("ERROR: patch output not right");
                return -1;
            }

            outputFile = outputFile.Substring(0, idx).Trim();
            var contentFile = Path.GetTempFileName();
            var patchFile = Path.GetTempFileName();

            File.WriteAllLines(contentFile, content);
            File.WriteAllLines(patchFile, patch);

            // patch -o output.txt -D AS_FOOBAR content.txt patch.txt
            var exec = new Executor(Directory.GetCurrentDirectory());
            if (exec.ExecuteProcess("patch", "-o", $"\"{outputFile}\"", "-D", define, $"\"{contentFile}\"", $"\"{patchFile}\"") != 0)
            {
                return -1;
            }

            return 0;
        }
    }
}
