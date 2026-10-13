namespace DanmaobErp.Domain.Plans;

public static class QuotaDefinitions
{
    public static IReadOnlyList<QuotaDefinition> All { get; } = Create();

    public static QuotaDefinition For(QuotaCode code)
    {
        return All.Single(d => d.Code == code);
    }

    private static List<QuotaDefinition> Create()
    {
        var definitions = new List<QuotaDefinition>();
        definitions.Add(new QuotaDefinition(QuotaCode.Tenants, QuotaKind.Count, QuotaScope.Account, false));
        definitions.Add(new QuotaDefinition(QuotaCode.ActiveUsers, QuotaKind.Count, QuotaScope.Account, false));
        definitions.Add(new QuotaDefinition(QuotaCode.Branches, QuotaKind.Count, QuotaScope.Tenant, false));
        definitions.Add(new QuotaDefinition(QuotaCode.Warehouses, QuotaKind.Count, QuotaScope.Tenant, false));
        definitions.Add(new QuotaDefinition(QuotaCode.Products, QuotaKind.Count, QuotaScope.Tenant, false));
        definitions.Add(new QuotaDefinition(QuotaCode.Customers, QuotaKind.Count, QuotaScope.Tenant, false));
        definitions.Add(new QuotaDefinition(QuotaCode.Prospects, QuotaKind.Count, QuotaScope.Tenant, false));
        definitions.Add(new QuotaDefinition(QuotaCode.SalesDocuments, QuotaKind.MonthlyConsumption, QuotaScope.Tenant, false));
        definitions.Add(new QuotaDefinition(QuotaCode.FileStorageBytes, QuotaKind.Count, QuotaScope.Account, false));
        definitions.Add(new QuotaDefinition(QuotaCode.InvoiceStamps, QuotaKind.MonthlyConsumption, QuotaScope.Tenant, false));
        definitions.Add(new QuotaDefinition(QuotaCode.BusinessEmails, QuotaKind.MonthlyConsumption, QuotaScope.Account, false));
        definitions.Add(new QuotaDefinition(QuotaCode.TaxDocumentStorageBytes, QuotaKind.Count, QuotaScope.Tenant, true));
        definitions.Add(new QuotaDefinition(QuotaCode.AuditQueryWindowDays, QuotaKind.Setting, QuotaScope.Tenant, false));
        return definitions;
    }
}

