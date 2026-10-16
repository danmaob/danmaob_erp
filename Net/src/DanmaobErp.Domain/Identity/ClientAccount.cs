using DanmaobErp.Domain.Auditing;
using DanmaobErp.Domain.Common;

namespace DanmaobErp.Domain.Identity;

public sealed class ClientAccount : Entity, IAuditable, IPlatformAuditScope
{
    public Guid PlanId { get; private set; }
    public AccountStatus Status { get; private set; }

    public ClientAccount(Guid planId)
    {
        if (planId == Guid.Empty)
        {
            throw new ArgumentException("The plan is required.", nameof(planId));
        }

        PlanId = planId;
        Status = AccountStatus.Active;
    }

    private ClientAccount()
    {
    }

    Guid? IPlatformAuditScope.AuditClientAccountId
    {
        get
        {
            return Id;
        }
    }

    Guid? IPlatformAuditScope.AuditTenantId
    {
        get
        {
            return null;
        }
    }
}
