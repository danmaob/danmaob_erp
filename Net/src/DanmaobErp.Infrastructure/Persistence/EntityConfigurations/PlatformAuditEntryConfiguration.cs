using DanmaobErp.Domain.Auditing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DanmaobErp.Infrastructure.Persistence.EntityConfigurations;

public sealed class PlatformAuditEntryConfiguration : IEntityTypeConfiguration<PlatformAuditEntry>
{
    public void Configure(EntityTypeBuilder<PlatformAuditEntry> builder)
    {
        builder.ToTable("PlatformAuditEntries", DatabaseSchemas.Platform);
        builder.Property(e => e.Id);
        builder.Property(e => e.ClientAccountId);
        builder.Property(e => e.TenantId);
        builder.Property(e => e.OccurredAt);
        builder.Property(e => e.ActorType).HasConversion<string>().HasMaxLength(20);
        builder.Property(e => e.ActorId);
        builder.Property(e => e.EntityType).HasMaxLength(100);
        builder.Property(e => e.EntityId);
        builder.Property(e => e.Operation).HasConversion<string>().HasMaxLength(20);
        builder.Property(e => e.BeforeState);
        builder.Property(e => e.AfterState);
        builder.HasIndex(e => e.OccurredAt);
        builder.HasIndex(e => new { e.EntityType, e.EntityId });
        builder.HasIndex(e => new { e.ClientAccountId, e.OccurredAt });
    }
}
