using System.Text.Json;
using System.Text.Json.Serialization;
using ErrorOr;

namespace Shopizy.Infrastructure.Common.Caching;

/// <summary>
/// JSON converter factory that handles ErrorOr&lt;T&gt; serialization for Redis caching.
/// Only the inner value is serialized; error results should not be cached.
/// </summary>
public sealed class ErrorOrConverterFactory : JsonConverterFactory
{
    public override bool CanConvert(Type typeToConvert) =>
        typeToConvert.IsGenericType
        && typeToConvert.GetGenericTypeDefinition() == typeof(ErrorOr<>);

    public override JsonConverter? CreateConverter(
        Type typeToConvert,
        JsonSerializerOptions options
    )
    {
        var valueType = typeToConvert.GetGenericArguments()[0];
        return (JsonConverter)
            Activator.CreateInstance(typeof(ErrorOrConverter<>).MakeGenericType(valueType))!;
    }
}

public sealed class ErrorOrConverter<T> : JsonConverter<ErrorOr<T>>
{
    public override ErrorOr<T> Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options
    )
    {
        var value = JsonSerializer.Deserialize<T>(ref reader, options);
        return value!;
    }

    public override void Write(
        Utf8JsonWriter writer,
        ErrorOr<T> errorOr,
        JsonSerializerOptions options
    ) =>
        // Serialize only the inner value; error results should not be cached.
        JsonSerializer.Serialize(writer, errorOr.IsError ? default : errorOr.Value, options);
}
