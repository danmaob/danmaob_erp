using DanmaobErp.Domain.Auditing;
using DanmaobErp.Domain.Plans;
using DanmaobErp.Infrastructure.Persistence;
using DanmaobErp.Infrastructure.Persistence.Seeding;
using DanmaobErp.Infrastructure.Tests.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DanmaobErp.Infrastructure.Tests.Plans;

public sealed class PlanQuotaSeedTests
{
    private static ErpDbContext CreateSeededContext()
    {
        var builder = new DbContextOptionsBuilder<ErpDbContext>();
        builder.UseInMemoryDatabase(Guid.NewGuid().ToString());
        var context = new ErpDbContext(builder.Options, new FixedTenantContext(null), new FixedCurrentActor(ActorType.Anonymous, null), new FixedBusinessClock(new DateTimeOffset(2026, 10, 12, 18, 0, 0, TimeSpan.Zero)));
        context.Database.EnsureCreated();
        return context;
    }

    private static Dictionary<QuotaCode, long?> LimitsOf(ErpDbContext context, Guid planId)
    {
        return context.Set<PlanQuota>().Where(q => q.PlanId == planId).ToDictionary(q => q.Quota, q => q.Limit);
    }

    [Fact]
    public void Free_MatchesTheRequirements()
    {
        using var context = CreateSeededContext();
        var expected = new Dictionary<QuotaCode, long?>();
        expected.Add(QuotaCode.Tenants, 1);
        expected.Add(QuotaCode.ActiveUsers, 2);
        expected.Add(QuotaCode.Branches, 1);
        expected.Add(QuotaCode.Warehouses, 1);
        expected.Add(QuotaCode.Products, 300);
        expected.Add(QuotaCode.Customers, 200);
        expected.Add(QuotaCode.Prospects, 100);
        expected.Add(QuotaCode.SalesDocuments, 100);
        expected.Add(QuotaCode.FileStorageBytes, 524288000);
        expected.Add(QuotaCode.InvoiceStamps, 0);
        expected.Add(QuotaCode.BusinessEmails, 0);
        expected.Add(QuotaCode.TaxDocumentStorageBytes, 0);
        expected.Add(QuotaCode.AuditQueryWindowDays, 90);
        Assert.Equal(expected, LimitsOf(context, PlanSeedData.FreePlanId));
    }

    [Fact]
    public void Basic_MatchesTheRequirements()
    {
        using var context = CreateSeededContext();
        var expected = new Dictionary<QuotaCode, long?>();
        expected.Add(QuotaCode.Tenants, 1);
        expected.Add(QuotaCode.ActiveUsers, 5);
        expected.Add(QuotaCode.Branches, 2);
        expected.Add(QuotaCode.Warehouses, 3);
        expected.Add(QuotaCode.Products, 5000);
        expected.Add(QuotaCode.Customers, 5000);
        expected.Add(QuotaCode.Prospects, 1000);
        expected.Add(QuotaCode.SalesDocuments, 2000);
        expected.Add(QuotaCode.FileStorageBytes, 5368709120);
        expected.Add(QuotaCode.InvoiceStamps, 100);
        expected.Add(QuotaCode.BusinessEmails, 1000);
        expected.Add(QuotaCode.TaxDocumentStorageBytes, 2147483648);
        expected.Add(QuotaCode.AuditQueryWindowDays, 365);
        Assert.Equal(expected, LimitsOf(context, PlanSeedData.BasicPlanId));
    }

    [Fact]
    public void Premium_MatchesTheApprovedValues()
    {
        using var context = CreateSeededContext();
        var expected = new Dictionary<QuotaCode, long?>();
        expected.Add(QuotaCode.Tenants, 3);
        expected.Add(QuotaCode.ActiveUsers, 15);
        expected.Add(QuotaCode.Branches, 10);
        expected.Add(QuotaCode.Warehouses, 10);
        expected.Add(QuotaCode.Products, 50000);
        expected.Add(QuotaCode.Customers, 50000);
        expected.Add(QuotaCode.Prospects, null);
        expected.Add(QuotaCode.SalesDocuments, 20000);
        expected.Add(QuotaCode.FileStorageBytes, 26843545600);
        expected.Add(QuotaCode.InvoiceStamps, 500);
        expected.Add(QuotaCode.BusinessEmails, 5000);
        expected.Add(QuotaCode.TaxDocumentStorageBytes, 10737418240);
        expected.Add(QuotaCode.AuditQueryWindowDays, null);
        Assert.Equal(expected, LimitsOf(context, PlanSeedData.PremiumPlanId));
    }

    [Fact]
    public void SalesDocuments_ReturnOnCancel_OnlyInPaidPlans()
    {
        using var context = CreateSeededContext();
        var returns = context.Set<PlanQuota>().Where(q => q.ReturnsOnCancel).Select(q => q.PlanId).OrderBy(id => id).ToList();
        Assert.Equal(new Guid[] { PlanSeedData.BasicPlanId, PlanSeedData.PremiumPlanId }, returns);
        Assert.Equal(2, context.Set<PlanQuota>().Count(q => q.Quota == QuotaCode.SalesDocuments && q.ReturnsOnCancel));
    }
}
