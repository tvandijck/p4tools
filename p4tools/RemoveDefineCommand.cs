using Spectre.Console;
using Spectre.Console.Cli;
using System.Diagnostics.CodeAnalysis;

namespace p4tools
{
    internal class RemoveDefineCommand : Command<RemoveDefineCommand.Settings>
    {
        public class Settings : CommandSettings
        {
            [CommandOption("-d|--define")]
            public string? Define { get; set; }

            public override ValidationResult Validate()
            {
                if (string.IsNullOrEmpty(Define))
                {
                    return ValidationResult.Error("Must set Define [-d|--define]");
                }
                return base.Validate();
            }
        }

        public override int Execute([NotNull] CommandContext context, [NotNull] Settings settings)
        {
            var cwd = Directory.GetCurrentDirectory();
            foreach (var file in Directory.EnumerateFiles(cwd, "*.as", SearchOption.AllDirectories))
            {
                RemoveDefine(file, settings.Define!);
            }
            return 0;
        }

        private void RemoveDefine(string file, string define)
        {
            var lines = File.ReadAllLines(file);

            using (var output = new InMemoryTextFile(file))
            {
                bool copy = true;
                int indefine = 0;
                foreach (var line in lines)
                {
                    var directive = line.TrimStart();
                    if (directive.Length > 0 && directive[0] == '#')
                    {
                        if (indefine > 0)
                        {
                            if (line.StartsWith($"#else"))
                            {
                                copy = !copy;
                            }
                            else if (line.StartsWith("#endif"))
                            {
                                copy = true;
                                indefine--;
                            }
                        }
                        else
                        {
                            if (line.StartsWith($"#ifdef {define}"))
                            {
                                copy = true;
                                indefine++;
                            }
                            else if (line.StartsWith($"#ifndef {define}"))
                            {
                                copy = false;
                                indefine++;
                            }
                            else if (copy)
                            {
                                output.WriteLine(line);
                            }
                        }
                    }
                    else
                    {
                        if (copy)
                        {
                            output.WriteLine(line);
                        }
                    }
                }
            }
        }
    }
}
