using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql;
using System.Text;

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
        NpgsqlConnection.GlobalTypeMapper.MapEnum<TypeOfData>("typeofdata");
        NpgsqlConnection.GlobalTypeMapper.MapEnum<Area>("area");
        NpgsqlConnection.GlobalTypeMapper.MapEnum<Range>("range");
        NpgsqlConnection.GlobalTypeMapper.MapEnum<Service>("service");
        NpgsqlConnection.GlobalTypeMapper.MapEnum<Geometry_Type>("geometry_type");
        //Npgsql.NpgsqlConnection.GlobalTypeMapper.UseNetTopologySuite();
        //Npgsql.NpgsqlConnection.GlobalTypeMapper.MapEnum<DeliveryType>("delivery_type");
#pragma warning restore CS0618
    }

    private partial void OnModelCreatingPartial(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<GeothermalParameter>(entity =>
        {
            entity.Property(p => p.Type).HasColumnName("typeofdata");
            entity.Property(p => p.Area).HasColumnName("area");
            entity.Property(p => p.Range).HasColumnName("range");
            entity.Property(p => p.Service).HasColumnName("service");
            entity.Property(p => p.Geometry_Type).HasColumnName("geometry_type");
        });
    }
}