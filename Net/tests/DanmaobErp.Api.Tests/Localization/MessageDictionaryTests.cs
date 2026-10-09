using System.Globalization;
using System.Reflection;
using DanmaobErp.Api.Tests.Errors.TestSupport;
using DanmaobErp.Application.Localization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;

namespace DanmaobErp.Api.Tests.Localization;

public sealed class MessageDictionaryTests
{
    private static List<string> GetAllKeys()
    {
        var keys = new List<string>();
        foreach (var nestedType in typeof(MessageKeys).GetNestedTypes())
        {
            foreach (var field in nestedType.GetFields(BindingFlags.Public | BindingFlags.Static))
            {
                if (field.IsLiteral && field.GetRawConstantValue() is string key)
                {
                    keys.Add(key);
                }
            }
        }
        return keys;
    }

    [Fact]
    public void EveryMessageKey_ExistsInSpanishDictionary()
    {
        CultureInfo.CurrentUICulture = new CultureInfo("es");
        using var factory = TestApi.Create();
        var localizer = factory.Services.GetRequiredService<IStringLocalizer>();
        var keys = GetAllKeys();
        Assert.NotEmpty(keys);
        var missing = keys.Where(key => localizer[key].ResourceNotFound).ToList();
        Assert.Empty(missing);
    }

    [Fact]
    public void EveryMessage_IsAValidFormatString()
    {
        CultureInfo.CurrentUICulture = new CultureInfo("es");
        using var factory = TestApi.Create();
        var localizer = factory.Services.GetRequiredService<IStringLocalizer>();
        foreach (var key in GetAllKeys())
        {
            var value = localizer[key].Value;
            Record.Exception(() => string.Format(CultureInfo.InvariantCulture, value, "field", 1));
        }
    }
}
