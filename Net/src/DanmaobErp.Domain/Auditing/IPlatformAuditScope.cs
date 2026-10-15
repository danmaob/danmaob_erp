namespace DanmaobErp.Domain.Auditing;

public interface IPlatformAuditScope
{
    public Guid? AuditClientAccountId { get; }
    public Guid? AuditTenantId { get; }
}
