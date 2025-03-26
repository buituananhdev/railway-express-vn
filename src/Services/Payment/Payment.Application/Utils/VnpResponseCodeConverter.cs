using System.Text.Json;
using System.Text.Json.Serialization;
using Payment.Domain.Enums;

namespace Payment.Application.Utils;
public class VnpResponseCodeConverter : JsonConverter<VnpResponseCode?>
{
    public override VnpResponseCode? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        switch (reader.TokenType)
        {
            case JsonTokenType.Number:
                int numericValue = reader.GetInt32();
                return Enum.IsDefined(typeof(VnpResponseCode), numericValue)
                    ? (VnpResponseCode)numericValue
                    : null;

            case JsonTokenType.String:
                string? stringValue = reader.GetString();

                if (int.TryParse(stringValue, out int parsedValue))
                {
                    return Enum.IsDefined(typeof(VnpResponseCode), parsedValue)
                        ? (VnpResponseCode)parsedValue
                        : null;
                }

                if (Enum.TryParse(stringValue, out VnpResponseCode enumValue))
                {
                    return enumValue;
                }

                return null;

            default:
                return null;
        }
    }

    public override void Write(Utf8JsonWriter writer, VnpResponseCode? value, JsonSerializerOptions options)
    {
        if (value.HasValue)
        {
            writer.WriteNumberValue((int)value.Value);
        }
        else
        {
            writer.WriteNullValue();
        }
    }
}
