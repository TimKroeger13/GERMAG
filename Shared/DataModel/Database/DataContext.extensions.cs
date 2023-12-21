using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace GERMAG.DataModel.Database;

public partial class DataContext : DbContext
{
    [Obsolete]
    public DataContext(DbContextOptions<DataContext> options) : base(options)
    {
        Initialize();
    }

    [Obsolete]
    private static void Initialize()
    {
        Npgsql.NpgsqlConnection.GlobalTypeMapper.UseNetTopologySuite();
    }

    private partial void OnModelCreatingPartial(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<GeothermalParameter>(entity =>
        {
            entity.Property(p => p.Geometry).HasColumnName("geometry").HasColumnName("geom");
        });
    }
}