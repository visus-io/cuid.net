#pragma warning disable CS0618 
#pragma warning disable VISLIB0001

namespace Visus.Cuid.Serialization.Json.Converters;

using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;

/// <summary>
///     Converter for serializing and deserializing <see cref="Cuid" /> values to and from JSON.
/// </summary>
public sealed class CuidConverter : JsonConverter<Cuid>
{
    /// <inheritdoc />
    public override Cuid Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        string valueAsString = reader.GetString();

        return string.IsNullOrEmpty(valueAsString)
                   ? Cuid.Empty
                   : Cuid.Parse(valueAsString);
    }

    /// <inheritdoc />
    public override void Write([NotNull] Utf8JsonWriter writer, Cuid value, JsonSerializerOptions options)
    {
        if ( value == Cuid.Empty )
        {
            writer.WriteNullValue();
        }
        else
        {
            writer.WriteStringValue(value.ToString());
        }
    }
}
