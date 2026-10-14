using DanmaobErp.Domain.Plans;
using DanmaobErp.Infrastructure.Persistence.Seeding;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DanmaobErp.Infrastructure.Persistence.EntityConfigurations;

public sealed class PlanConfiguration : IEntityTypeConfiguration<Plan>
{
    public void Configure(EntityTypeBuilder<Plan> builder)
    {
        builder.ToTable("Plans", DatabaseSchemas.Platform);
        builder.Property(e => e.Id);
        builder.Property(e => e.IsActive);
        builder.Property(e => e.Name).HasMaxLength(50);
        builder.Property(e => e.IsDefaultForRegistration);
        builder.HasIndex(e => e.Name).IsUnique();
        builder.HasIndex(e => e.IsDefaultForRegistration).IsUnique().HasFilter("[IsDefaultForRegistration] = 1");
        builder.HasData(PlanSeedData.Plans());
    }
}
