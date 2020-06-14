using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Crosberg.SignalChop
{
	public class DynamicJsonConverter : JsonConverter<dynamic>
	{
		public override dynamic Read(ref Utf8JsonReader reader,
			Type typeToConvert,
			JsonSerializerOptions options)
		{
			switch (reader.TokenType)
			{
				case JsonTokenType.True:
					return true;
				case JsonTokenType.False:
					return false;
				case JsonTokenType.Number when reader.TryGetInt64(out long l):
					return l;
				case JsonTokenType.Number:
					return reader.GetDouble();
				case JsonTokenType.String when reader.TryGetDateTime(out DateTime datetime):
					return datetime;
				case JsonTokenType.String:
					return reader.GetString();
				case JsonTokenType.StartObject:
					return this.ReadObject(JsonDocument.ParseValue(ref reader).RootElement);
				default:
					return JsonDocument.ParseValue(ref reader).RootElement.Clone();
			}
		}

		public override void Write(Utf8JsonWriter writer, object value, JsonSerializerOptions options)
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