using DanmaobErp.Domain.Auditing;
using DanmaobErp.Domain.Plans;

namespace DanmaobErp.Infrastructure.Persistence;

public static class PlatformCatalog
{
    public static IReadOnlyList<Type> EntityTypes { get; } = new Type[] { typeof(PlatformAuditEntry), typeof(Plan), typeof(PlanModule), typeof(PlanQuota) };
}
