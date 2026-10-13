using DanmaobErp.Domain.Common;

namespace DanmaobErp.Domain.Plans;

public sealed class PlanModule : Entity, IAuditable
{
    public Guid PlanId { get; private set; }

    public ModuleCode Module { get; private set; }

    public PlanModule(Guid planId, ModuleCode module)
    {
        if (planId == Guid.Empty)
        {
            throw new ArgumentException("The plan is required.", nameof(planId));
        }

        PlanId = planId;
        Module = module;
    }

    private PlanModule()
    {
    }
}
