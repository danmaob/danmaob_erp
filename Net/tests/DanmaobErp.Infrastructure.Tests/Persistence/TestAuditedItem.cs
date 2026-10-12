using DanmaobErp.Domain.Common;

namespace DanmaobErp.Infrastructure.Tests.Persistence;

public sealed class TestAuditedItem : SoftDeletableEntity, ITenantOwned, IAuditable
{
    public Guid TenantId { get; private set; }

    public string Name { get; private set; }

    [Sensitive]
    public string Secret { get; private set; }

    public TestAuditedItem(string name, string secret)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(secret);
        Name = name;
        Secret = secret;
    }

    private TestAuditedItem()
    {
        Name = string.Empty;
        Secret = string.Empty;
    }

    public void Rename(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        Name = name;
    }
}
