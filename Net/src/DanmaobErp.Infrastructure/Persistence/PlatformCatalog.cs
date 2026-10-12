using DanmaobErp.Domain.Auditing;

namespace DanmaobErp.Infrastructure.Persistence;

public static class PlatformCatalog
{
    public static IReadOnlyList<Type> EntityTypes { get; } = new Type[] { typeof(PlatformAuditEntry) };
}
