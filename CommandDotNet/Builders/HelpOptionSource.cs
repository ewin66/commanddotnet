﻿using System;
using System.Linq;
using CommandDotNet.Execution;
using CommandDotNet.Help;

namespace CommandDotNet.Builders
{
    internal class HelpOptionSource : IOptionSource
    {
        public void AddOption(ICommandBuilder commandBuilder)
        {
            var option = new Option(Constants.HelpTemplate, ArgumentArity.Zero)
            {
                Description = "Show help information",
                TypeDisplayName = Constants.TypeDisplayNames.Flag,
                IsSystemOption = true,
                Arity = ArgumentArity.Zero
            };

            commandBuilder.AddArgument(option);
        }
        
        internal static int HelpMiddleware(CommandContext commandContext, Func<CommandContext, int> next)
        {
            if (commandContext.ParseResult.Values.Any(v => v.Argument.Name == Constants.HelpArgumentTemplate.Name))
            {
                Print(commandContext.AppSettings, commandContext.ParseResult.Command);
                return 0;
            }

            return next(commandContext);
        }

        public static void Print(AppSettings appSettings, ICommand command)
        {
            IHelpProvider helpTextProvider = HelpTextProviderFactory.Create(appSettings);
            appSettings.Out.WriteLine(helpTextProvider.GetHelpText(command));
        }
    }
}