﻿using System.Threading.Tasks;
using CommandDotNet.Execution;
using CommandDotNet.Rendering;

namespace CommandDotNet.Builders
{
    internal static class VersionMiddleware
    {
        internal const string VersionOptionName = "version";

        internal static AppRunner UseVersionMiddleware(AppRunner appRunner)
        {
            return appRunner.Configure(c =>
            {
                c.BuildEvents.OnCommandCreated += AddVersionOption;
                c.UseMiddleware(DisplayVersionIfSpecified, MiddlewareStages.ParseInput, int.MaxValue-100);
            });
        }

        private static void AddVersionOption(BuildEvents.CommandCreatedEventArgs args)
        {
            if (!args.CommandBuilder.Command.IsRootCommand())
            {
                return;
            }

            if (args.CommandBuilder.Command.ContainsArgumentNode(VersionOptionName))
            {
                return;
            }

            var option = new Option(VersionOptionName, 'v', 
                args.CommandBuilder.Command, TypeInfo.Flag, ArgumentArity.Zero, 
                definitionSource: typeof(VersionMiddleware).FullName)
            {
                Description = "Show version information",
                IsMiddlewareOption = true,
                Arity = ArgumentArity.Zero
            };

            args.CommandBuilder.AddArgument(option);
        }

        private static Task<int> DisplayVersionIfSpecified(CommandContext commandContext,
            ExecutionDelegate next)
        {
            if (commandContext.RootCommand.HasRawValues(VersionOptionName))
            {
                Print(commandContext, commandContext.Console);
                return Task.FromResult(0);
            }

            return next(commandContext);
        }

        private static void Print(CommandContext commandContext, IConsole console)
        {
            var versionInfo = VersionInfo.GetVersionInfo(commandContext);

            console.Out.WriteLine(versionInfo.Filename);
            console.Out.WriteLine(versionInfo.Version);
        }
    }
}