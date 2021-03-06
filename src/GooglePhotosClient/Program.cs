﻿namespace GooglePhotosClient
{
    using GooglePhotosClient.Assets;
    using Microsoft.Extensions.CommandLineUtils;
    using System;
    using System.Reflection;
    using C = Colorful;

    public class Program
    {
        public static void Main(string[] args)
        {
            C.Console.WriteAscii(General.Name);
            var app = new CommandLineApplication(false)
            {
                Name = General.Name,
                ShortVersionGetter = () => Assembly.GetEntryAssembly().ImageRuntimeVersion
            };

            app.HelpOption(CommandNames.Help);
            app.Command(CommandNames.Downsync, x =>
            {
                x.Description = CommandNames.DownsyncDescription;
                var clientSecretPath = x.Option(
                    CommandNames.ClientSecretPathArgumentTemplate,
                    CommandNames.ClientSecretPathArgumentDescription,
                    CommandOptionType.SingleValue);
                var clientSecretSavePath = x.Option(
                    CommandNames.ClientSecretSaveArgumentTemplate,
                    CommandNames.ClientSecretSaveArgumentDescription,
                    CommandOptionType.SingleValue);
                var backupPath = x.Option(
                    CommandNames.BackupPathArgumentTemplate,
                    CommandNames.BackupPathArgumentDescription,
                    CommandOptionType.SingleValue);
                var since = x.Option(
                    CommandNames.SinceArgumentTemplate,
                    CommandNames.SinceArgumentDescription,
                    CommandOptionType.SingleValue);
                x.OnExecute(() =>
                {
                    var command = new DownsyncCommand(clientSecretSavePath, clientSecretPath, backupPath, since);
                    return command.Validate() ? command.Execute() : -1;
                });
                x.HelpOption(CommandNames.Help);
            });

            if (args.Length == 0) 
            {
                app.ShowHelp();
            }

            app.Execute(args);
        }
    }
}
