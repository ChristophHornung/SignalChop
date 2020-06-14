using System;
using System.Threading.Tasks;

namespace Crosberg.SignalChop
{
	internal class SendCommand : SignalChopCommand
	{
		public override string Name => "Send";

		public override void DisplayHelp()
		{
			Console.WriteLine("Usage: Send <method> [parameter1] [parameter2] ...");
			Console.WriteLine("Description: Sends a SignalR invocation message for <method> to the server.");
			Console.WriteLine(
				"\t[parameterX] defines the parameters of the invocation. The number of parameters has to match the <method>s definition on the server.");
			Console.WriteLine(
				"\tUse '-marks to denote strings or json. Json parameters have to start with a curly bracket ({).");
			Console.WriteLine(
				"Example: Send Order 'Nike' 10 {\"ProductName\":\"Shoe\", \"Id\":2, \"Comment\":\"Pink laces\"}'");
		}

		public override async Task Execute(SignalChopper chopper, string[] args)
		{
			if (args.Length < 1)
			{
				await Console.Error.WriteLineAsync("Missing method name for send command.");
				return;
			}

			chopper.Send(args[0], args[1..]);
		}
	}
}