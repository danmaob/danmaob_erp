using DanmaobErp.Domain.Plans;

namespace DanmaobErp.Infrastructure.Tests.Plans;

public sealed class QuotaDefinitionsTests
{
    [Fact]
    public void EveryQuotaCode_HasExactlyOneDefinition()
    {
        var codes = Enum.GetValues<QuotaCode>();
        Assert.Equal(codes.Length, QuotaDefinitions.All.Count);
        foreach (var code in codes)
        {
            Assert.Single(QuotaDefinitions.All, d => d.Code == code);
        }
    }

    [Fact]
    public void OnlyTaxDocumentStorage_WarnsWithoutBlocking_AndAuditWindowIsASetting()
    {
        var warnOnly = Assert.Single(QuotaDefinitions.All, d => d.WarnOnly);
        Assert.Equal(QuotaCode.TaxDocumentStorageBytes, warnOnly.Code);
        Assert.Equal(QuotaKind.Setting, QuotaDefinitions.For(QuotaCode.AuditQueryWindowDays).Kind);
    }
}
