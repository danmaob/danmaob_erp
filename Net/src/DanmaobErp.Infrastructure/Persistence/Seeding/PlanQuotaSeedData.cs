using DanmaobErp.Domain.Plans;

namespace DanmaobErp.Infrastructure.Persistence.Seeding;

public static class PlanQuotaSeedData
{
    public static object[] PlanQuotas()
    {
        var quotas = new List<object>();
        quotas.Add(Quota(1, PlanSeedData.FreePlanId, QuotaCode.Tenants, 1, false));
        quotas.Add(Quota(1, PlanSeedData.FreePlanId, QuotaCode.ActiveUsers, 2, false));
        quotas.Add(Quota(1, PlanSeedData.FreePlanId, QuotaCode.Branches, 1, false));
        quotas.Add(Quota(1, PlanSeedData.FreePlanId, QuotaCode.Warehouses, 1, false));
        quotas.Add(Quota(1, PlanSeedData.FreePlanId, QuotaCode.Products, 300, false));
        quotas.Add(Quota(1, PlanSeedData.FreePlanId, QuotaCode.Customers, 200, false));
        quotas.Add(Quota(1, PlanSeedData.FreePlanId, QuotaCode.Prospects, 100, false));
        quotas.Add(Quota(1, PlanSeedData.FreePlanId, QuotaCode.SalesDocuments, 100, false));
        quotas.Add(Quota(1, PlanSeedData.FreePlanId, QuotaCode.FileStorageBytes, 524288000, false));
        quotas.Add(Quota(1, PlanSeedData.FreePlanId, QuotaCode.InvoiceStamps, 0, false));
        quotas.Add(Quota(1, PlanSeedData.FreePlanId, QuotaCode.BusinessEmails, 0, false));
        quotas.Add(Quota(1, PlanSeedData.FreePlanId, QuotaCode.TaxDocumentStorageBytes, 0, false));
        quotas.Add(Quota(1, PlanSeedData.FreePlanId, QuotaCode.AuditQueryWindowDays, 90, false));
        quotas.Add(Quota(2, PlanSeedData.BasicPlanId, QuotaCode.Tenants, 1, false));
        quotas.Add(Quota(2, PlanSeedData.BasicPlanId, QuotaCode.ActiveUsers, 5, false));
        quotas.Add(Quota(2, PlanSeedData.BasicPlanId, QuotaCode.Branches, 2, false));
        quotas.Add(Quota(2, PlanSeedData.BasicPlanId, QuotaCode.Warehouses, 3, false));
        quotas.Add(Quota(2, PlanSeedData.BasicPlanId, QuotaCode.Products, 5000, false));
        quotas.Add(Quota(2, PlanSeedData.BasicPlanId, QuotaCode.Customers, 5000, false));
        quotas.Add(Quota(2, PlanSeedData.BasicPlanId, QuotaCode.Prospects, 1000, false));
        quotas.Add(Quota(2, PlanSeedData.BasicPlanId, QuotaCode.SalesDocuments, 2000, true));
        quotas.Add(Quota(2, PlanSeedData.BasicPlanId, QuotaCode.FileStorageBytes, 5368709120, false));
        quotas.Add(Quota(2, PlanSeedData.BasicPlanId, QuotaCode.InvoiceStamps, 100, false));
        quotas.Add(Quota(2, PlanSeedData.BasicPlanId, QuotaCode.BusinessEmails, 1000, false));
        quotas.Add(Quota(2, PlanSeedData.BasicPlanId, QuotaCode.TaxDocumentStorageBytes, 2147483648, false));
        quotas.Add(Quota(2, PlanSeedData.BasicPlanId, QuotaCode.AuditQueryWindowDays, 365, false));
        quotas.Add(Quota(3, PlanSeedData.PremiumPlanId, QuotaCode.Tenants, 3, false));
        quotas.Add(Quota(3, PlanSeedData.PremiumPlanId, QuotaCode.ActiveUsers, 15, false));
        quotas.Add(Quota(3, PlanSeedData.PremiumPlanId, QuotaCode.Branches, 10, false));
        quotas.Add(Quota(3, PlanSeedData.PremiumPlanId, QuotaCode.Warehouses, 10, false));
        quotas.Add(Quota(3, PlanSeedData.PremiumPlanId, QuotaCode.Products, 50000, false));
        quotas.Add(Quota(3, PlanSeedData.PremiumPlanId, QuotaCode.Customers, 50000, false));
        quotas.Add(Quota(3, PlanSeedData.PremiumPlanId, QuotaCode.Prospects, null, false));
        quotas.Add(Quota(3, PlanSeedData.PremiumPlanId, QuotaCode.SalesDocuments, 20000, true));
        quotas.Add(Quota(3, PlanSeedData.PremiumPlanId, QuotaCode.FileStorageBytes, 26843545600, false));
        quotas.Add(Quota(3, PlanSeedData.PremiumPlanId, QuotaCode.InvoiceStamps, 500, false));
        quotas.Add(Quota(3, PlanSeedData.PremiumPlanId, QuotaCode.BusinessEmails, 5000, false));
        quotas.Add(Quota(3, PlanSeedData.PremiumPlanId, QuotaCode.TaxDocumentStorageBytes, 10737418240, false));
        quotas.Add(Quota(3, PlanSeedData.PremiumPlanId, QuotaCode.AuditQueryWindowDays, null, false));
        return quotas.ToArray();
    }

    private static object Quota(int planNumber, Guid planId, QuotaCode quota, long? limit, bool returnsOnCancel)
    {
        return new { Id = PlanSeedData.SeedId(planNumber, 2, (int)quota), PlanId = planId, Quota = quota, Limit = limit, ReturnsOnCancel = returnsOnCancel };
    }
}
