using DanmaobErp.Domain.Plans;
using DanmaobErp.Infrastructure.Persistence.Seeding;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DanmaobErp.Infrastructure.Persistence.EntityConfigurations;

public sealed class PlanModuleConfiguration : IEntityTypeConfiguration<PlanModule>
{
    public void Configure(EntityTypeBuilder<PlanModule> builder)
    {
        builder.ToTable("PlanModules", DatabaseSchemas.Platform);
        builder.Property(e => e.Id);
        builder.Property(e => e.PlanId);
        builder.Property(e => e.Module).HasConversion<string>().HasMaxLength(30);
        builder.HasOne<Plan>().WithMany().HasForeignKey(e => e.PlanId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(e => new { e.PlanId, e.Module }).IsUnique();
        builder.HasData(PlanSeedData.PlanModules());
    }
}
