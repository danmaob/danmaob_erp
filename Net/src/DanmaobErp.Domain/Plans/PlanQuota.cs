using DanmaobErp.Domain.Common;

namespace DanmaobErp.Domain.Plans;

public sealed class PlanQuota : Entity, IAuditable
{
    public Guid PlanId { get; private set; }
    
    public QuotaCode Quota { get; private set; }
    
    public long? Limit { get; private set; }
    
    public bool ReturnsOnCancel { get; private set; }

    public PlanQuota(Guid planId, QuotaCode quota, long? limit, bool returnsOnCancel)
    {
        if (planId == Guid.Empty)
        {
            throw new ArgumentException("The plan is required.", nameof(planId));
        }

        if (limit < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(limit), "The limit cannot be negative.");
        }

        PlanId = planId;
        Quota = quota;
        Limit = limit;
        ReturnsOnCancel = returnsOnCancel;
    }

    private PlanQuota()
    {
    }
}
