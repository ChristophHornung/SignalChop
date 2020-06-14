using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Crosberg.SignalChop
{
	internal class HelpCommand : SignalChopCommand
	{
		private readonly Dictionary<string, SignalChopCommand> commands;

		public HelpCommand(Dictionary<string, SignalChopCommand> commands)
		{
			this.commands = commands;
		}

		public override string Name => "Help";

		public override void DisplayHelp()
		{
			Console.WriteLine("Usage: Help <method>");
			Console.WriteLine("Description: Displays detailed help for the given <method>.");
			Console.WriteLine("Example: Help Connect");
		}

		public override async Task Execute(SignalChopper chopper, string[] args)
		{
			if (args.Length < 1)
			{
				await Console.Error.WriteLineAsync(
					"Missing method name for help command. Use Help <method>, e.g. 'Help Listen'.");
				return;
			}

			if (this.commands.ContainsKey(args[0]))
			{
				this.commands[args[0]].DisplayHelp();
			}
			else
			{
				await Console.Error.WriteLineAsync($"Unknown command '{args[0]}'.");
			}
		}
	}
}