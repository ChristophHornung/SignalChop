namespace Crosberg.SignalChop;

using System;
using System.Threading.Tasks;

internal class ListenCommand : SignalChopCommand
{
	public override string Name => "Listen";

	public override void DisplayHelp()
	{
		Console.WriteLine("Usage: Listen <method> [parameter1] [parameter2]...");
		Console.WriteLine(
			"Description: Listens for SignalR invocations for the given <method>. Received messages will be output on the console in JSON format.");
		Console.WriteLine(
			"\t [parameterX] will be used to name the given parameter in the json output. The number of parameters has to match the <method>s definition on the server.");
		Console.WriteLine("Example: Listen broadcastMessage username message");
	}

	public override Task Execute(SignalChopper chopper, string[] args)
	{
		chopper.Listen(args);
		return Task.CompletedTask;
	}
}