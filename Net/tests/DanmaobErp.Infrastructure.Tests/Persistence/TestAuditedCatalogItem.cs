using DanmaobErp.Domain.Common;

namespace DanmaobErp.Infrastructure.Tests.Persistence;

public sealed class TestAuditedCatalogItem : Entity, IAuditable
{
    public string Name { get; private set; }

    [Sensitive]
    public string Secret { get; private set; }

    public TestAuditedCatalogItem(string name, string secret)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(secret);
        Name = name;
        Secret = secret;
    }

    private TestAuditedCatalogItem()
    {
        Name = string.Empty;
        Secret = string.Empty;
    }
}
