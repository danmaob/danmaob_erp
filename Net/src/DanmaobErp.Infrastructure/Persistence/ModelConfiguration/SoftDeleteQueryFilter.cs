using System.Linq.Expressions;
using DanmaobErp.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace DanmaobErp.Infrastructure.Persistence.ModelConfiguration;

public static class SoftDeleteQueryFilter
{
    public const string FilterName = "SoftDelete";

    public static void Apply(ModelBuilder modelBuilder)
    {
        var entityTypes = modelBuilder.Model.GetEntityTypes().ToList();

        foreach (var entityType in entityTypes)
        {
            if (!typeof(SoftDeletableEntity).IsAssignableFrom(entityType.ClrType))
            {
                continue;
            }

            if (entityType.BaseType is not null)
            {
                continue;
            }

            var parameter = Expression.Parameter(entityType.ClrType, "entity");
            var body = Expression.Property(parameter, nameof(SoftDeletableEntity.IsActive));
            var filter = Expression.Lambda(body, parameter);
            modelBuilder.Entity(entityType.ClrType).HasQueryFilter(FilterName, filter);
        }
    }
}
