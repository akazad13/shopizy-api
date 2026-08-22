using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace Shopizy.Infrastructure.Common.Caching;

/// <summary>
/// A System.Text.Json TypeInfoResolver that automatically enables deserialization into properties
/// with private/protected setters and private backing fields, completely eliminating the need for
/// [JsonInclude] attributes across all domain entities.
/// </summary>
public class PrivateSetterContractResolver : DefaultJsonTypeInfoResolver
{
    public override JsonTypeInfo GetTypeInfo(Type type, JsonSerializerOptions options)
    {
        var jsonTypeInfo = base.GetTypeInfo(type, options);

        if (jsonTypeInfo.Kind == JsonTypeInfoKind.Object)
        {
            // 1. Enable private setters for public properties
            foreach (var property in jsonTypeInfo.Properties)
            {
                if (property.Set == null)
                {
                    var propertyInfo = type.GetProperty(
                        property.Name,
                        BindingFlags.Instance
                            | BindingFlags.Public
                            | BindingFlags.NonPublic
                            | BindingFlags.IgnoreCase
                    );

                    if (propertyInfo?.SetMethod != null)
                    {
                        property.Set = propertyInfo.SetValue;
                    }
                }
            }

            // 2. Automatically include non-public backing fields (e.g., _productImages, _cartItems)
            var privateFields = type.GetFields(BindingFlags.Instance | BindingFlags.NonPublic);
            foreach (var field in privateFields)
            {
                if (
                    field.Name.StartsWith('_')
                    && !jsonTypeInfo.Properties.Any(p =>
                        p.Name.Equals(field.Name, StringComparison.OrdinalIgnoreCase)
                    )
                )
                {
                    var propertyInfo = jsonTypeInfo.CreateJsonPropertyInfo(
                        field.FieldType,
                        field.Name
                    );
                    propertyInfo.Get = field.GetValue;
                    propertyInfo.Set = field.SetValue;
                    jsonTypeInfo.Properties.Add(propertyInfo);
                }
            }
        }

        return jsonTypeInfo;
    }
}
