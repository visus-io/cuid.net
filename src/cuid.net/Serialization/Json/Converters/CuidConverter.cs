#if NETSTANDARD2_0 || NET472
#pragma warning disable CS0618 // Type or member is obsolete
#endif
#pragma warning disable VISLIB0001
namespace Visus.Cuid.Serialization.Json.Converters;

using System;
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
