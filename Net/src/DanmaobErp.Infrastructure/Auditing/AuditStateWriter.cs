using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using DanmaobErp.Domain.Common;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace DanmaobErp.Infrastructure.Auditing;

public static class AuditStateWriter
{
    public const string MaskedValue = "***";

    private static readonly JsonSerializerOptions SerializerOptions = CreateSerializerOptions();

    public static string Write(EntityEntry entry, bool useOriginalValues, bool onlyModified)
    {
        var values = new Dictionary<string, object?>();

        foreach (var property in entry.Properties)
        {
            if (property.Metadata.IsShadowProperty())
            {
                continue;
            }

            if (onlyModified && property.IsModified is false)
            {
                continue;
            }

            if (property.Metadata.PropertyInfo?.IsDefined(typeof(SensitiveAttribute), true) is true)
            {
                values[property.Metadata.Name] = MaskedValue;
                continue;
            }

            values[property.Metadata.Name] = useOriginalValues ? property.OriginalValue : property.CurrentValue;
        }

        return JsonSerializer.Serialize(values, SerializerOptions);
    }

    private static JsonSerializerOptions CreateSerializerOptions()
    {
        var options = new JsonSerializerOptions();
        options.Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping;
        options.Converters.Add(new JsonStringEnumConverter());
        return options;
    }
}
