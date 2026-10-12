using DanmaobErp.Domain.Auditing;

namespace DanmaobErp.Infrastructure.Auditing;

public static class AuditExclusions
{
    public static IReadOnlyList<Type> EntityTypes { get; } = new Type[] { typeof(TenantAuditEntry), typeof(PlatformAuditEntry) };
}
