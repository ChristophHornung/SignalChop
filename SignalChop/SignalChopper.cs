namespace Crosberg.SignalChop;

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;

internal class SignalChopper
{
	private readonly Dictionary<string, IDisposable> listeningEntries = [];
	private HubConnection? connection;
	public int WaitCount { get; set; }
	public bool VerboseMode { get; set; }
	public bool QuiteMode { get; set; }

	[MemberNotNullWhen(true, nameof(this.connection))]
	public bool CheckConnection()
	{
		if (this.connection?.State != HubConnectionState.Connected)
		{
			Console.Error.WriteLine(
				"Not connected to server or server currently trying to reconnect. Use Connect <server> to connect.");
			return false;
		}

		return true;
	}

	public async Task Build(string host)
	{
		IHubConnectionBuilder builder = new HubConnectionBuilder()
			.WithUrl(host)
			.WithAutomaticReconnect();
		if (this.VerboseMode)
		{
			builder.ConfigureLogging(logging => logging.AddConsole());
		}

		this.connection = builder.Build();
		this.connection.Closed += this.HandleClosed;
		this.connection.Reconnected += this.HandleReconnected;
		this.connection.Reconnecting += this.HandleReconnecting;
		await this.connection.StartAsync();
		if (!this.QuiteMode)
		{
			Console.Error.WriteLine(this.connection.State);
		}
	}

	public void Quit(int newWaitCount)
	{
		this.WaitCount = newWaitCount;
		if (newWaitCount == 0)
		{
			this.connection?.DisposeAsync();
			Environment.Exit(0);
		}
	}

	public async Task Connect(string server)
	{
		try
		{
			await this.Build(server);
		}
		catch (Exception e)
		{
			await Console.Error.WriteLineAsync(e.Message);
		}
	}

	public async Task Send(string method, string[] args)
	{
		if (!this.CheckConnection())
		{
			return;
		}

		await this.connection.SendCoreAsync(method, this.ConvertSendArguments(args));
		if (!this.QuiteMode)
		{
			await Console.Error.WriteLineAsync($"Message for {method} sent.");
		}
	}

	public async Task<dynamic?> Invoke(string method, string[] args)
	{
		if (!this.CheckConnection())
		{
			return null;
		}

		dynamic? result = await this.connection.InvokeCoreAsync<dynamic?>(method, this.ConvertSendArguments(args));
		if (!this.QuiteMode)
		{
			await Console.Error.WriteLineAsync($"Message for {method} sent.");
		}

		return result;
	}

	public void Listen(string[] splitCommand)
	{
		if (!this.CheckConnection())
		{
			return;
		}

		this.StopListen(splitCommand);

		IDisposable listeningOnEntry =
			this.connection.On(splitCommand[0],
				Enumerable.Range(0, splitCommand.Length - 1).Select(m => typeof(object)).ToArray(),
				d => this.OnReceived(splitCommand, d));

		this.listeningEntries[splitCommand[0]] = listeningOnEntry;

		if (!this.QuiteMode)
		{
			Console.Error.WriteLine($"Listening on {splitCommand[0]} with {splitCommand.Length - 1} parameter.");
		}
	}

	public void StopListen(string[] splitCommand)
	{
		if (this.listeningEntries.TryGetValue(splitCommand[0], out var priorListener))
		{
			priorListener.Dispose();
		}
	}

	private async Task HandleReconnecting(Exception? arg)
	{
		if (!this.QuiteMode)
		{
			await Console.Error.WriteLineAsync("Trying to reconnect to server: " + arg?.Message);
		}
	}

	private async Task HandleReconnected(string? arg)
	{
		if (!this.QuiteMode)
		{
			await Console.Error.WriteLineAsync("Connection to server reconnected.");
		}
	}

	private async Task HandleClosed(Exception? e)
	{
		if (!this.QuiteMode)
		{
			if (e != null)
			{
				await Console.Error.WriteLineAsync("Connection closed with error: " + e.Message);
			}
			else
			{
				await Console.Error.WriteLineAsync("Connection closed");
			}
		}
	}

	private object[] ConvertSendArguments(string[] args)
	{
		object[] result = new object[args.Length];
		for (int i = 0; i < args.Length; i++)
		{
			string arg = args[i].Trim('\'');
			if (arg.Trim().StartsWith('{'))
			{
				result[i] = JsonSerializer.Deserialize<dynamic>(arg, new JsonSerializerOptions()
				{
					Converters = { new DynamicJsonConverter() }
				})!;
			}
			else
			{
				result[i] = arg;
			}
		}

		return result;
	}

	private Task OnReceived(string[] splitCommand, object[] data)
	{
		Console.WriteLine($"{{ \"message\":\"{splitCommand[0]}\",");
		Console.WriteLine("\"data\":{");
		for (int i = 0; i < splitCommand[1..].Length; i++)
		{
			string param = splitCommand[1..][i];
			if (data[i] is JsonElement { ValueKind: JsonValueKind.String } s)
			{
				Console.Write($"\"{param}\" : \"{s}\"");
			}
			else
			{
				Console.Write($"\"{param}\" : {data[i]}");
			}

			if (i == splitCommand[1..].Length - 1)
			{
				Console.WriteLine();
			}
			else
			{
				Console.WriteLine(",");
			}
		}

		Console.WriteLine("}}");
		if (this.WaitCount > 0)
		{
			this.Quit(--this.WaitCount);
		}

		return Task.CompletedTask;
	}
}