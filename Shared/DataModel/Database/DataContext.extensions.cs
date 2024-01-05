using Microsoft.EntityFrameworkCore;

namespace GERMAG.DataModel.Database;

public partial class DataContext : DbContext
{
    public DataContext(DbContextOptions<DataContext> options) : base(options)
    {
        Initialize();
    }

    private static void Initialize()
    {
#pragma warning disable CS0618 // Type or member is obsolete
        //Npgsql.NpgsqlConnection.GlobalTypeMapper.MapEnum<DeliveryType>("delivery_type");
#pragma warning restore CS0618 // Type or member is obsolete
    }

    private partial void OnModelCreatingPartial(ModelBuilder modelBuilder)
    {
        //modelBuilder.Entity<GeothermalParameter>(entity =>
        //{
        //    entity.Property(p => p.Geom).HasColumnName("geometry").HasColumnName("geom");
        //});
    }
}