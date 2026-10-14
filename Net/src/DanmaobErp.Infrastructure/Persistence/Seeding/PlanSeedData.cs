using DanmaobErp.Domain.Plans;

namespace DanmaobErp.Infrastructure.Persistence.Seeding;

public static class PlanSeedData
{
    public static Guid FreePlanId { get; } = SeedId(1, 0, 0);

    public static Guid BasicPlanId { get; } = SeedId(2, 0, 0);

    public static Guid PremiumPlanId { get; } = SeedId(3, 0, 0);

    public static Guid SeedId(int planNumber, int group, int index)
    {
        return Guid.Parse($"019a0000-0000-7000-8000-{planNumber:D4}{group:D2}{index:D6}");
    }

    public static object[] Plans()
    {
        var plans = new List<object>();
        plans.Add(new { Id = FreePlanId, Name = "Free", IsDefaultForRegistration = true, IsActive = true });
        plans.Add(new { Id = BasicPlanId, Name = "Básico", IsDefaultForRegistration = false, IsActive = true });
        plans.Add(new { Id = PremiumPlanId, Name = "Premium", IsDefaultForRegistration = false, IsActive = false });
        return plans.ToArray();
    }

    public static object[] PlanModules()
    {
        var modules = new List<object>();
        modules.Add(Module(1, FreePlanId, ModuleCode.Customers));
        modules.Add(Module(1, FreePlanId, ModuleCode.Inventory));
        modules.Add(Module(1, FreePlanId, ModuleCode.Sales));
        modules.Add(Module(2, BasicPlanId, ModuleCode.Customers));
        modules.Add(Module(2, BasicPlanId, ModuleCode.Inventory));
        modules.Add(Module(2, BasicPlanId, ModuleCode.Sales));
        modules.Add(Module(2, BasicPlanId, ModuleCode.Purchasing));
        modules.Add(Module(2, BasicPlanId, ModuleCode.Invoicing));
        modules.Add(Module(2, BasicPlanId, ModuleCode.Finance));
        modules.Add(Module(2, BasicPlanId, ModuleCode.Reports));
        modules.Add(Module(2, BasicPlanId, ModuleCode.BusinessEmail));
        modules.Add(Module(3, PremiumPlanId, ModuleCode.Customers));
        modules.Add(Module(3, PremiumPlanId, ModuleCode.Inventory));
        modules.Add(Module(3, PremiumPlanId, ModuleCode.Sales));
        modules.Add(Module(3, PremiumPlanId, ModuleCode.Purchasing));
        modules.Add(Module(3, PremiumPlanId, ModuleCode.Invoicing));
        modules.Add(Module(3, PremiumPlanId, ModuleCode.Finance));
        modules.Add(Module(3, PremiumPlanId, ModuleCode.Reports));
        modules.Add(Module(3, PremiumPlanId, ModuleCode.BusinessEmail));
        return modules.ToArray();
    }

    private static object Module(int planNumber, Guid planId, ModuleCode module)
    {
        return new { Id = SeedId(planNumber, 1, (int)module), PlanId = planId, Module = module };
    }
}
