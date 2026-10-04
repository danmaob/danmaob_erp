using System.Linq.Expressions;
using DanmaobErp.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace DanmaobErp.Infrastructure.Persistence.ModelConfiguration;

public static class TenantQueryFilter
{
    public const string FilterName = "Tenant";

    public static void Apply(ModelBuilder modelBuilder, ErpDbContext context)
    {
        var entityTypes = modelBuilder.Model.GetEntityTypes().ToList();

        foreach (var entityType in entityTypes)
        {
            if (!typeof(ITenantOwned).IsAssignableFrom(entityType.ClrType))
            {
                continue;
            }

            if (entityType.BaseType is not null)
            {
                continue;
            }

            var parameter = Expression.Parameter(entityType.ClrType, "entity");
            var entityTenant = Expression.Property(parameter, nameof(ITenantOwned.TenantId));
            var currentTenant = Expression.Property(Expression.Constant(context), nameof(ErpDbContext.CurrentTenantId));
            var body = Expression.Equal(entityTenant, currentTenant);
            var filter = Expression.Lambda(body, parameter);
            modelBuilder.Entity(entityType.ClrType).HasQueryFilter(FilterName, filter);
        }
    }
}
