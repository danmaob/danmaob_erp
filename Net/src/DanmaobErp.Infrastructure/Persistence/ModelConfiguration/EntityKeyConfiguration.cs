using DanmaobErp.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace DanmaobErp.Infrastructure.Persistence.ModelConfiguration;

public static class EntityKeyConfiguration
{
    public const string ClusterKeyPropertyName = "ClusterKey";

    public static void Apply(ModelBuilder modelBuilder)
    {
        var entityTypes = modelBuilder.Model.GetEntityTypes().ToList();

        foreach (var entityType in entityTypes)
        {
            if (!typeof(Entity).IsAssignableFrom(entityType.ClrType))
            {
                continue;
            }

            if (entityType.BaseType is not null)
            {
                continue;
            }

            var entityBuilder = modelBuilder.Entity(entityType.ClrType);
            entityBuilder.Property(nameof(Entity.Id)).ValueGeneratedNever();
            entityBuilder.HasKey(nameof(Entity.Id)).IsClustered(false);
            entityBuilder.Property<long>(ClusterKeyPropertyName).ValueGeneratedOnAdd().UseIdentityColumn();
            entityBuilder.HasIndex(ClusterKeyPropertyName).IsUnique().IsClustered();
        }
    }
}
