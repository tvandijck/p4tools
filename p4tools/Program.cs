using Spectre.Console.Cli;
using System;

namespace p4tools
{
    internal class Program
    {
        static int Main(string[] args)
        {
            var app = new CommandApp();
            app.Configure(config =>
            {
                config.AddCommand<UpdateGitCommand>("git-update");
                config.AddCommand<ProtectAngelscriptCommand>("as-protect");
                //config.AddCommand<RebaseCommand>("rebase");
            });

            return app.Run(args);
        }
    }
}