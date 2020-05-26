using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace Crosberg.SignalChop.Example.Hubs
{
	public class ExampleEchoHub : Hub
	{
		public async Task SendComplexObject(string echo, MessageParameter message)
		{
			// Echo the message back
			await this.Clients.All.SendAsync("broadcastMessage", echo, 1, message);
			//await this.Clients.All.SendAsync("broadcastEchoOnly", echo);
		}
	}

	public class MessageParameter
	{
		public int Id { get; set; }
		public MessageSenderParameter Sender { get; set; }
		public string Message { get; set; }
	}

	public class MessageSenderParameter
	{
		public int Id { get; set; }
		public string Sender { get; set; }
	}
}