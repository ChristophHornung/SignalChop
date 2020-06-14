using System;
using System.Threading.Tasks;

namespace Crosberg.SignalChop
{
	internal class StopListenCommand : SignalChopCommand
	{
		public override string Name => "StopListen";

		public override void DisplayHelp()
		{
			Console.WriteLine("Usage: StopListen <method>");
			Console.WriteLine("Description: Stops listening for SignalR invocation messages for <method>.");
			Console.WriteLine("Example: StopListen broadcastMessage");
		}

		public override Task Execute(SignalChopper chopper, string[] args)
		{
			if (args.Length < 1)
			{
				Console.WriteLine("Missing method name.");
				this.DisplayHelp();
			}
			else
			{
				chopper.CheckConnection();
				chopper.StopListen(args);
			}

			return Task.CompletedTask;
		}
	}
}