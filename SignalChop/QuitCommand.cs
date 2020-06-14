using System;
using System.Threading.Tasks;

namespace Crosberg.SignalChop
{
	internal class QuitCommand : SignalChopCommand
	{
		public override string Name => "Quit";

		public override void DisplayHelp()
		{
			Console.WriteLine("Usage: Quit [waitCount]");
			Console.WriteLine(
				"Description: Quits all execution and disconnects from the server. " +
				"The optional [waitCount] indicates to not quit immediately but instead wait for [waitCount] invocations from the server first and then quit.");
			Console.WriteLine("Example: Quit 2");
		}

		public override Task Execute(SignalChopper chopper, string[] args)
		{
			chopper.Quit(args.Length > 0 ? int.Parse(args[0]) : 0);
			return Task.CompletedTask;
		}
	}
}