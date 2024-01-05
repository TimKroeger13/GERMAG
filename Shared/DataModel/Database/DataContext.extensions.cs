using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace GERMAG.DataModel.Database;

public partial class DataContext : DbContext
{
    public DataContext(DbContextOptions<DataContext> options) : base(options)
    {
        Initialize();
    }

    private static void Initialize()
    {
#pragma warning disable CS0618
        //Npgsql.NpgsqlConnection.GlobalTypeMapper.UseNetTopologySuite();
        //Npgsql.NpgsqlConnection.GlobalTypeMapper.MapEnum<DeliveryType>("delivery_type");
#pragma warning restore CS0618
    }

    private partial void OnModelCreatingPartial(ModelBuilder modelBuilder)
    {
        /*
        modelBuilder.Entity<GeothermalParameter>(entity =>
        {
            entity.Property(p => p.Geom).HasColumnName("geometry").HasColumnName("geom");
        });
        */
    }
}