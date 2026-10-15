using DanmaobErp.Domain.Plans;
using DanmaobErp.Infrastructure.Persistence.Seeding;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DanmaobErp.Infrastructure.Persistence.EntityConfigurations;

public sealed class PlanQuotaConfiguration : IEntityTypeConfiguration<PlanQuota>
{
    public void Configure(EntityTypeBuilder<PlanQuota> builder)
    {
        builder.ToTable("PlanQuotas", DatabaseSchemas.Platform);
        builder.Property(e => e.Id);
        builder.Property(e => e.PlanId);
        builder.Property(e => e.Quota).HasConversion<string>().HasMaxLength(40);
        builder.Property(e => e.Limit);
        builder.Property(e => e.ReturnsOnCancel);
        builder.HasOne<Plan>().WithMany().HasForeignKey(e => e.PlanId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(e => new { e.PlanId, e.Quota }).IsUnique();
        builder.HasData(PlanQuotaSeedData.PlanQuotas());
    }
}
