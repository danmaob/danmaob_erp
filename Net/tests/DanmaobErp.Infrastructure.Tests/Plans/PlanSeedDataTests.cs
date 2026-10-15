using DanmaobErp.Domain.Auditing;
using DanmaobErp.Domain.Plans;
using DanmaobErp.Infrastructure.Persistence;
using DanmaobErp.Infrastructure.Persistence.Seeding;
using DanmaobErp.Infrastructure.Tests.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DanmaobErp.Infrastructure.Tests.Plans;

public sealed class PlanSeedDataTests
{
    private static ErpDbContext CreateSeededContext()
    {
        var builder = new DbContextOptionsBuilder<ErpDbContext>();
        builder.UseInMemoryDatabase(Guid.NewGuid().ToString());
        var context = new ErpDbContext(builder.Options, new FixedTenantContext(null), new FixedCurrentActor(ActorType.Anonymous, null), new FixedBusinessClock(new DateTimeOffset(2026, 10, 12, 18, 0, 0, TimeSpan.Zero)));
        context.Database.EnsureCreated();
        return context;
    }

    [Fact]
    public void Free_IsTheOnlyDefaultPlanForRegistration()
    {
        using var context = CreateSeededContext();
        var plan = Assert.Single(context.Set<Plan>().IgnoreQueryFilters().Where(p => p.IsDefaultForRegistration).ToList());
        Assert.Equal(PlanSeedData.FreePlanId, plan.Id);
        Assert.Equal("Free", plan.Name);
    }

    [Fact]
    public void Premium_IsSeededInactive()
    {
        using var context = CreateSeededContext();
        var premium = context.Set<Plan>().IgnoreQueryFilters().Single(p => p.Id == PlanSeedData.PremiumPlanId);
        Assert.False(premium.IsActive);
        Assert.Equal(2, context.Set<Plan>().Count());
    }

    [Fact]
    public void Free_HasOnlyCustomersInventoryAndSales()
    {
        using var context = CreateSeededContext();
        var modules = context.Set<PlanModule>().Where(m => m.PlanId == PlanSeedData.FreePlanId).Select(m => m.Module).OrderBy(m => m).ToList();
        Assert.Equal(new ModuleCode[] { ModuleCode.Customers, ModuleCode.Inventory, ModuleCode.Sales }, modules);
    }

    [Fact]
    public void PaidPlans_HaveEveryModule()
    {
        using var context = CreateSeededContext();
        var moduleCount = Enum.GetValues<ModuleCode>().Length;
        Assert.Equal(moduleCount, context.Set<PlanModule>().Count(m => m.PlanId == PlanSeedData.BasicPlanId));
        Assert.Equal(moduleCount, context.Set<PlanModule>().Count(m => m.PlanId == PlanSeedData.PremiumPlanId));
    }

    [Fact]
    public void EveryPlan_HasOneQuotaPerQuotaCode()
    {
        using var context = CreateSeededContext();
        var quotaCount = Enum.GetValues<QuotaCode>().Length;
        Assert.Equal(quotaCount, context.Set<PlanQuota>().Count(q => q.PlanId == PlanSeedData.FreePlanId));
        Assert.Equal(quotaCount, context.Set<PlanQuota>().Count(q => q.PlanId == PlanSeedData.BasicPlanId));
        Assert.Equal(quotaCount, context.Set<PlanQuota>().Count(q => q.PlanId == PlanSeedData.PremiumPlanId));
    }
}
