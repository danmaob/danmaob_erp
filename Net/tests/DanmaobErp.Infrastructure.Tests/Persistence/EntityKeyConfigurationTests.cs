using DanmaobErp.Infrastructure.Persistence;
using DanmaobErp.Infrastructure.Persistence.ModelConfiguration;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;

namespace DanmaobErp.Infrastructure.Tests.Persistence;

public sealed class EntityKeyConfigurationTests
{
    [Fact]
    public void NewEntity_HasVersion7Id()
    {
        var item = new TestItem("One");

        Assert.NotEqual(Guid.Empty, item.Id);
        Assert.Equal(7, item.Id.Version);
    }

    [Fact]
    public void Model_UsesNonClusteredKeyAndClusteredClusterKeyIndex()
    {
        var builder = new DbContextOptionsBuilder<ErpDbContext>();
        builder.UseInMemoryDatabase(Guid.NewGuid().ToString());
        using var context = new TestErpDbContext(builder.Options);
        var model = context.GetService<IDesignTimeModel>().Model;
        var entityType = model.FindEntityType(typeof(TestItem));

        Assert.NotNull(entityType);
        var primaryKey = entityType.FindPrimaryKey();
        Assert.NotNull(primaryKey);
        Assert.False(primaryKey.IsClustered());
        var clusterKey = entityType.FindProperty(EntityKeyConfiguration.ClusterKeyPropertyName);
        Assert.NotNull(clusterKey);
        Assert.Equal(typeof(long), clusterKey.ClrType);
        var index = entityType.FindIndex(clusterKey);
        Assert.NotNull(index);
        Assert.True(index.IsUnique);
        Assert.True(index.IsClustered());
    }
}
