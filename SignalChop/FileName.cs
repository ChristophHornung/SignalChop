namespace Crosberg.SignalChop;

using Microsoft.AspNetCore.Connections;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Protocol;
using Microsoft.Extensions.Options;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

internal sealed class CatchAllJsonHubProtocol : IHubProtocol
{
	public const string CatchAllTarget = "$__catchAll";
	private readonly JsonHubProtocol inner;

	public CatchAllJsonHubProtocol(IOptions<JsonHubProtocolOptions> options)
		=>
			this.inner = new JsonHubProtocol(options);

	/// <inheritdoc/>
	public string Name => this.inner.Name;

	/// <inheritdoc/>
	public int Version => this.inner.Version;

	/// <inheritdoc/>
	public TransferFormat TransferFormat => this.inner.TransferFormat;

	/// <inheritdoc/>
	public bool IsVersionSupported(int version) => this.inner.IsVersionSupported(version);

	/// <inheritdoc/>
	public ReadOnlyMemory<byte> GetMessageBytes(HubMessage message) => this.inner.GetMessageBytes(message);

	/// <inheritdoc/>
	public void WriteMessage(HubMessage message, IBufferWriter<byte> output) =>
		this.inner.WriteMessage(message, output);

	/// <inheritdoc/>
	public bool TryParseMessage(ref ReadOnlySequence<byte> input, IInvocationBinder binder, [NotNullWhen(true)] out HubMessage? message)
	{
		ReadOnlySequence<byte> original = input;

		// JSON frames are terminated by ASCII record separator 0x1E. :contentReference[oaicite:2]{index=2}
		if (!TryReadFrame(ref input, out ReadOnlySequence<byte> payload))
		{
			input = original;
			message = null;
			return false;
		}

		// Peek into the JSON to decide if we should reroute.
		try
		{
			using JsonDocument doc = JsonDocument.Parse(payload.ToArray());
			JsonElement root = doc.RootElement;

			// Invocation message type is 1
			if (root.TryGetProperty("type", out JsonElement typeEl) &&
			    typeEl.ValueKind == JsonValueKind.Number &&
			    typeEl.GetInt32() == 1 &&
			    root.TryGetProperty("target", out JsonElement targetEl) &&
			    targetEl.ValueKind == JsonValueKind.String)
			{
				string target = targetEl.GetString()!;
				int argCount = 0;
				if (root.TryGetProperty("arguments", out JsonElement argsEl) && argsEl.ValueKind == JsonValueKind.Array)
				{
					argCount = argsEl.GetArrayLength();
				}

				bool known = true;
				int expected = 0;

				try
				{
					IReadOnlyList<Type>? paramTypes = binder.GetParameterTypes(target);
					expected = paramTypes?.Count ?? 0;
				}
				catch
				{
					known = false;
				}

				if (!known || expected != argCount)
				{
					string? invocationId =
						root.TryGetProperty("invocationId", out JsonElement idEl) && idEl.ValueKind == JsonValueKind.String
							? idEl.GetString()
							: null;

					// Clone makes it safe after disposing the JsonDocument.
					JsonElement envelope = root.Clone();

					// InvocationMessage supports (invocationId, target, arguments). :contentReference[oaicite:3]{index=3}
					message = new InvocationMessage(invocationId, CatchAllTarget, [envelope]);
					return true;
				}
			}
		}
		catch
		{
			// If our peek fails, fall back to the normal parser below.
		}

		// Not a reroute case: let the normal JSON protocol parse it.
		input = original;
		return this.inner.TryParseMessage(ref input, binder, out message);
	}

	private static bool TryReadFrame(ref ReadOnlySequence<byte> input, out ReadOnlySequence<byte> payload)
	{
		SequenceReader<byte> reader = new(input);

		if (!reader.TryReadTo(out payload, 0x1E, advancePastDelimiter: true))
		{
			payload = default;
			return false;
		}

		input = input.Slice(reader.Position);
		return true;
	}
}