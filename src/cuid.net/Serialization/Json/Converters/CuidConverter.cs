#pragma warning disable VISLIB0001
namespace Visus.Cuid.Serialization.Json.Converters;

using System.Text.Json;
using System.Text.Json.Serialization;

/// <inheritdoc />
public class CuidConverter : JsonConverter<Cuid>
{
	/// <inheritdoc />
	public override Cuid Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		return Cuid.Parse(reader.GetString()!);
	}

	/// <inheritdoc />
	public override void Write(Utf8JsonWriter writer, Cuid value, JsonSerializerOptions options)
	{
		writer.WriteStringValue(value.ToString());
	}
}