﻿using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;

namespace Crosberg.SignalChop
{
	internal class Program
	{
		private HubConnection? connection;
		private bool exit;
		private int waitCount;
		private bool quiteMode;
		private readonly Dictionary<string, IDisposable> listeningEntries = new Dictionary<string, IDisposable>();

		/// <summary>
		/// A simple generic signalR sender/receiver.
		/// </summary>
		/// <param name="commandFile">An optional command file that will be executed line by line.</param>
		/// <param name="quite">Whether to output status information or restrict the output to received json only.</param>
		/// <param name="exitAfterCount">An integer to indicate how many messages to retrieve before quitting. 0 indicates that no auto-exit will occur.</param>
		/// <returns>The status code.</returns>
		private static async Task<int> Main(string? commandFile = null, bool quite = false, int exitAfterCount = 0)
		{
			return await new Program().Run(commandFile, quite, exitAfterCount);
		}

		private static string[] ParseMultiSpacedArguments(string commandLine)
		{
			var isLastCharSpace = false;
			char[] parmChars = commandLine.ToCharArray();
			bool inQuote = false;
			for (int index = 0; index < parmChars.Length; index++)
			{
				if (parmChars[index] == '\'')
				{
					inQuote = !inQuote;
				}

				if (!inQuote && parmChars[index] == ' ' && !isLastCharSpace)
				{
					parmChars[index] = '\n';
				}

				isLastCharSpace = parmChars[index] == '\n' || parmChars[index] == ' ';
			}

			return new string(parmChars).Split('\n');
		}

		private async Task<int> Run(string? commandFile, bool quite, int exitAfterCount)
		{
			this.waitCount = exitAfterCount;
			this.quiteMode = quite;
			if (!quite)
			{
				this.ShowGeneralHelp();
			}

			{
				try
				{
					if (!string.IsNullOrEmpty(commandFile))
					{
						foreach (var line in await File.ReadAllLinesAsync(commandFile))
						{
							await this.RunCommand(line);
						}
					}

					while (!this.exit)
					{
						await this.RunCommand(Console.ReadLine());
					}

					return 0;
				}
				catch (Exception e)
				{
					Console.Error.WriteLine(e);
					return 1;
				}
			}
		}

		private void ShowGeneralHelp()
		{
			Console.WriteLine(
				"Available Commands: Connect<server>; Listen <method>; Send <method> <parameters>[]; Help <method>; Quit [afterCountMessages]; ");
		}

		private async Task RunCommand(string line)
		{
			string[] splitCommand = ParseMultiSpacedArguments(line);
			switch (splitCommand[0])
			{
				case "StopListen":
					this.CheckConnection();
					this.StopListen(splitCommand[1..]);
					break;
				case "Listen":
					this.CheckConnection();
					this.Listen(splitCommand[1..]);
					break;
				case "Connect":
					await this.Connect(splitCommand[1]);
					break;
				case "Send":
					if (splitCommand.Length < 2)
					{
						await Console.Error.WriteLineAsync("Missing method name for send command.");
						return;
					}
					this.CheckConnection();
					this.Send(splitCommand[1], splitCommand[2..]);
					break;
				case "Help":
					if (splitCommand.Length < 2)
					{
						await Console.Error.WriteLineAsync(
							"Missing method name for help command. Use Help <method>, e.g. 'Help Listen'.");
						return;
					}

					await this.Help(splitCommand[1]);
					break;
				case "Quit":
					this.Quit(splitCommand.Length > 1 ? int.Parse(splitCommand[1]) : 0);
					break;
				default:
					await Console.Error.WriteLineAsync("Unknown command");
					this.ShowGeneralHelp();
					break;
			}
		}

		private void CheckConnection()
		{
			if (this.connection == null || this.connection.State!=HubConnectionState.Connected)
			{
				System.Console.Error.WriteLine(
					"Not connected to server or server currently trying to reconnect. Use Connect <server> to connect.");
			}
		}

		private async Task Help(string method)
		{
			switch (method)
			{
				case "StopListen":
					Console.WriteLine("Usage: StopListen <method>");
					Console.WriteLine("Description: Stops listening for SingalR invocation messages for <method>.");
					Console.WriteLine("Example: StopListen broadcastMessage");
					break;
				case "Listen":
					Console.WriteLine("Usage: Listen <method> [parameter1] [parameter2]...");
					Console.WriteLine(
						"Description: Listens for SignalR invocations for the given <method>. Received messages will be output on the console in JSON format.");
					Console.WriteLine(
						"\t [parameterX] will be used to name the given parameter in the json output. The number of parameters has to match the <method>s definition on the server.");
					Console.WriteLine("Example: Listen broadcastMessage username chatmessage");
					break;
				case "Connect":
					Console.WriteLine("Usage: Connect <server>");
					Console.WriteLine("Description: Connects to the given signalR server.");
					Console.WriteLine("Example: Connect https://localhost:50001/chatHub");
					break;
				case "Send":
					Console.WriteLine("Usage: Send <method> [parameter1] [parameter2] ...");
					Console.WriteLine("Description: Sends a SignalR invocation message for <method> to the server.");
					Console.WriteLine(
						"\t[parameterX] defines the parameters of the invocation. The number of parameters has to match the <method>s definition on the server.");
					Console.WriteLine(
						"\tUse '-marks to denote strings or json. Json parameters have to start with a curly bracket ({).");
					Console.WriteLine(
						"Example: Send Order 'Nike' 10 {\"ProductName\":\"Shoe\", \"Id\":2, \"Comment\":\"Pink laces\"}'");
					break;
				case "Help":
					Console.WriteLine("Usage: Help <method>");
					Console.WriteLine("Description: Displays detailed help for the given <method>.");
					Console.WriteLine("Example: Help Connect");
					break;
				case "Quit":
					Console.WriteLine("Usage: Quit [waitCount]");
					Console.WriteLine(
						"Description: Quits all execution and disconnects from the server. " +
						"The optional [waitCount] indicates to not quit immediately but instead wait for [waitCount] invocations from the server first and then quit.");
					Console.WriteLine("Example: Quit 2");
					break;
				default:
					await Console.Error.WriteLineAsync("Unknown command");
					this.ShowGeneralHelp();
					break;
			}
		}

		private async Task Connect(string server)
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

		private void Quit(int newWaitCount)
		{
			this.waitCount = newWaitCount;
			if (newWaitCount == 0)
			{
				this.connection?.DisposeAsync();
				this.exit = true;
			}
		}

		private void StopListen(string[] splitCommand)
		{
			if (this.listeningEntries.TryGetValue(splitCommand[0], out var priorListener))
			{
				priorListener.Dispose();
			}
		}

		private void Listen(string[] splitCommand)
		{
			this.StopListen(splitCommand);

			var listeningOnEntry = this.connection.On(splitCommand[0],
				Enumerable.Range(0, splitCommand.Length - 1).Select(m => typeof(object)).ToArray(),
				d => this.OnReceived(splitCommand, d));

			this.listeningEntries[splitCommand[0]] = listeningOnEntry;

			if (!this.quiteMode)
			{
				Console.WriteLine($"Listening on {splitCommand[0]}");
			}
		}

		private Task OnReceived(string[] splitCommand, object[] data)
		{
			Console.WriteLine($"{{ \"message\":\"{splitCommand[0]}\"");
			Console.WriteLine("\"data\":{");
			for (var i = 0; i < splitCommand[1..].Length; i++)
			{
				string param = splitCommand[1..][i];
				Console.WriteLine($"\"{param}\" : {data[i]}");
			}

			Console.WriteLine("}}");
			if (this.waitCount > 0)
			{
				this.Quit(this.waitCount--);
			}

			return Task.CompletedTask;
		}

		private async Task Build(string host)
		{
			var builder = new HubConnectionBuilder()
				.WithUrl(host)
				.ConfigureLogging(logging => { logging.AddConsole(); })
				.WithAutomaticReconnect();
			this.connection = builder.Build();
			this.connection.Closed += this.HandleClosed;
			this.connection.Reconnected += this.HandleReconnected;
			this.connection.Reconnecting += this.HandleReconnecting;
			await this.connection.StartAsync();
			if (!this.quiteMode)
			{
				Console.WriteLine(this.connection.State);
			}
		}

		private async Task HandleReconnecting(Exception arg)
		{
			if (!this.quiteMode)
			{
				await Console.Error.WriteLineAsync("Trying to reconnect to server: " + arg.Message);
			}
		}

		private async Task HandleReconnected(string arg)
		{
			if (!this.quiteMode)
			{
				await Console.Error.WriteLineAsync("Connection to server reconnected.");
			}
		}

		private async Task HandleClosed(Exception e)
		{
			if (!this.quiteMode)
			{
				await Console.Error.WriteLineAsync("Connection closed: " + e.Message);
			}
		}

		private async void Send(string method, string[] args)
		{
			await this.connection.InvokeCoreAsync(method, this.ConvertSendArguments(args));
			if (!this.quiteMode)
			{
				await Console.Error.WriteLineAsync($"Message for {method} sent.");
			}
		}

		private object[] ConvertSendArguments(string[] args)
		{
			object[] result = new object[args.Length];
			for (int i = 0; i < args.Length; i++)
			{
				string arg = args[i].Trim('\'');
				if (arg.Trim().StartsWith("{"))
				{
					result[i] = JsonSerializer.Deserialize<dynamic>(arg, new JsonSerializerOptions()
					{
						Converters = {new DynamicJsonConverter()}
					});
				}
				else
				{
					result[i] = arg[i];
				}
			}

			return result;
		}

		public class DynamicJsonConverter : JsonConverter<dynamic>
		{
			public override dynamic Read(ref Utf8JsonReader reader,
				Type typeToConvert,
				JsonSerializerOptions options)
			{
				if (reader.TokenType == JsonTokenType.True)
				{
					return true;
				}

				if (reader.TokenType == JsonTokenType.False)
				{
					return false;
				}

				if (reader.TokenType == JsonTokenType.Number)
				{
					if (reader.TryGetInt64(out long l))
					{
						return l;
					}

					return reader.GetDouble();
				}

				if (reader.TokenType == JsonTokenType.String)
				{
					if (reader.TryGetDateTime(out DateTime datetime))
					{
						return datetime;
					}

					return reader.GetString();
				}

				if (reader.TokenType == JsonTokenType.StartObject)
				{
					using JsonDocument documentV = JsonDocument.ParseValue(ref reader);
					return this.ReadObject(documentV.RootElement);
				}

				JsonDocument document = JsonDocument.ParseValue(ref reader);
				return document.RootElement.Clone();
			}

			public override void Write(Utf8JsonWriter writer,
				object value,
				JsonSerializerOptions options)
			{
				JsonSerializer.Serialize(writer, value, options);
			}

			private object ReadObject(JsonElement jsonElement)
			{
				IDictionary<string, object> expandoObject = new ExpandoObject();
				foreach (var obj in jsonElement.EnumerateObject())
				{
					var k = obj.Name;
					var value = this.ReadValue(obj.Value);
					if (value != null)
					{
						expandoObject[k] = value;
					}
				}

				return expandoObject;
			}

			private object? ReadValue(JsonElement jsonElement)
			{
				object? result;
				switch (jsonElement.ValueKind)
				{
					case JsonValueKind.Object:
						result = this.ReadObject(jsonElement);
						break;
					case JsonValueKind.Array:
						result = this.ReadList(jsonElement);
						break;
					case JsonValueKind.String:
						result = jsonElement.GetString();
						if (jsonElement.TryGetDateTime(out var dt))
						{
							result = dt;
						}

						if (jsonElement.TryGetDateTimeOffset(out var dto))
						{
							result = dto;
						}

						break;
					case JsonValueKind.Number:
						result = 0;
						if (jsonElement.TryGetInt64(out var l))
						{
							result = l;
						}

						if (jsonElement.TryGetDouble(out var d))
						{
							result = d;
						}

						if (jsonElement.TryGetDecimal(out var de))
						{
							result = de;
						}

						break;
					case JsonValueKind.True:
						result = true;
						break;
					case JsonValueKind.False:
						result = false;
						break;
					case JsonValueKind.Undefined:
					case JsonValueKind.Null:
						result = null;
						break;
					default:
						throw new ArgumentOutOfRangeException();
				}

				return result;
			}

			private object? ReadList(JsonElement jsonElement)
			{
				IList<object?> list = jsonElement.EnumerateArray().Select(this.ReadValue).ToList();
				return list.Count == 0 ? null : list;
			}
		}
	}
}