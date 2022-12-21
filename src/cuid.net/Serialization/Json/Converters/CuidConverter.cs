namespace Xaevik.Cuid.Serialization.Json.Converters;

using System.Text.Json;
using System.Text.Json.Serialization;

public class CuidConverter : JsonConverter<Cuid>
{
	public override Cuid Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		return Cuid.Parse(reader.GetString()!);
	}

	public override void Write(Utf8JsonWriter writer, Cuid value, JsonSerializerOptions options)
	{
		writer.WriteStringValue(value.ToString());
	}
}