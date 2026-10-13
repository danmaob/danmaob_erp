using DanmaobErp.Domain.Common;

namespace DanmaobErp.Domain.Plans;

public sealed class Plan : SoftDeletableEntity, IAuditable
{
    public string Name { get; private set; }
    public bool IsDefaultForRegistration { get; private set; }

    public Plan(string name, bool isDefaultForRegistration)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        Name = name;
        IsDefaultForRegistration = isDefaultForRegistration;
    }

    private Plan()
    {
        Name = string.Empty;
    }
}
