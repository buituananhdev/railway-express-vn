using Google.Protobuf.WellKnownTypes;

namespace Common.Infrastructure.Utils;
public static class StructExtensions
{
    public static Dictionary<string, object> ToDictionary(this Struct s)
    {
        return s.Fields.ToDictionary(
            kvp => kvp.Key,
            kvp => ConvertValue(kvp.Value)
        );
    }

    private static object ConvertValue(Value value)
    {
        switch (value.KindCase)
        {
            case Value.KindOneofCase.BoolValue:
                return value.BoolValue;
            case Value.KindOneofCase.NumberValue:
                return value.NumberValue;
            case Value.KindOneofCase.StringValue:
                return value.StringValue;
            case Value.KindOneofCase.StructValue:
                return value.StructValue.ToDictionary();
            case Value.KindOneofCase.ListValue:
                return value.ListValue.Values.Select(ConvertValue).ToList();
            case Value.KindOneofCase.NullValue:
                return null;
            default:
                return null;
        }
    }
}
