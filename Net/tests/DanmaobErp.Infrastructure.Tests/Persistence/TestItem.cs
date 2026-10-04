using DanmaobErp.Domain.Common;

namespace DanmaobErp.Infrastructure.Tests.Persistence;

public sealed class TestItem : SoftDeletableEntity
{
    public string Name { get; private set; }

    public TestItem(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        Name = name;
    }

    private TestItem()
    {
        Name = string.Empty;
    }
}
