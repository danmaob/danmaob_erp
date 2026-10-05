using DanmaobErp.Domain.Common;

namespace DanmaobErp.Infrastructure.Tests.Persistence;

public sealed class TestTenantItem : SoftDeletableEntity, ITenantOwned
{
    public Guid TenantId { get; private set; }

    public string Name { get; private set; }

    public TestTenantItem(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        Name = name;
    }

    private TestTenantItem()
    {
        Name = string.Empty;
    }
}
