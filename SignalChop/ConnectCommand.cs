namespace Crosberg.SignalChop;

using System;
using System.Threading.Tasks;

internal class ConnectCommand : SignalChopCommand
{
	public override string Name => "Connect";

	public override void DisplayHelp()
	{
		Console.WriteLine("Usage: Connect <server>");
		Console.WriteLine("Description: Connects to the given signalR server.");
		Console.WriteLine("Example: Connect https://localhost:50001/chatHub");
	}

	public override async Task Execute(SignalChopper chopper, string[] args)
	{
		if (args.Length < 1)
		{
			Console.WriteLine("Missing server name.");
			this.DisplayHelp();
			return;
		}

		await chopper.Connect(args[0]);
	}
}