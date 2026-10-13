namespace DanmaobErp.Domain.Plans;

public sealed class QuotaDefinition
{
    public QuotaCode Code { get; }

    public QuotaKind Kind { get; }

    public QuotaScope Scope { get; }

    public bool WarnOnly { get; }

    public QuotaDefinition(QuotaCode code, QuotaKind kind, QuotaScope scope, bool warnOnly)
    {
        Code = code;
        Kind = kind;
        Scope = scope;
        WarnOnly = warnOnly;
    }
}
