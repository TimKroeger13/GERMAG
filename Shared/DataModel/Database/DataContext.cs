using Microsoft.EntityFrameworkCore;

namespace GERMAG.DataModel.Database;

public partial class DataContext : DbContext
{
    public virtual DbSet<GeothermalParameter> GeothermalParameter { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .HasPostgresEnum("area", new[] { "Berlin" })
            .HasPostgresEnum("range", new[] { "NearRange", "FarRange" })
            .HasPostgresEnum("service", new[] { "Restrictive", "Efficiency" })
            .HasPostgresEnum("typeofdata", new[] { "LandParcels", "DGM", "GeoPotenRestrict", "VegHeight", "MainWaterLines", "GroundwaterSurfaceDistance", "GroundWaterHeightMain", "GroundWaterHeightTension", "WaterAmmonium", "WaterBor", "WaterChlor", "WaterKalium", "WaterSulfat", "WaterOrthoPhosphat", "ElectricalCon", "MeanWaterTemp_20_100", "MeanWaterTemp_20", "MeanWaterTemp_40", "MeanWaterTemp_60", "GeodrillingData", "GeologicalSections", "GeoDrawing", "WaterProtecAreas", "ExpeMaxGroundWaterHight", "GeoPoten_100m_2400ha", "GeoPoten_100m_1800ha", "GeoPoten_80m_2400ha", "GeoPoten_80m_1800ha", "GeoPoten_60m_2400ha", "GeoPoten_60m_1800ha", "GeoPoten_40m_2400ha", "GeoPoten_40m_1800h", "ThermalCon_40m", "ThermalCon_60m", "ThermalCon_80m", "ThermalCon_100m" })
            .HasPostgresExtension("fuzzystrmatch")
            .HasPostgresExtension("postgis")
            .HasPostgresExtension("postgis_raster")
            .HasPostgresExtension("tiger", "postgis_tiger_geocoder")
            .HasPostgresExtension("topology", "postgis_topology");

        modelBuilder.Entity<GeothermalParameter>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("geothermal_parameters_pkey");

            entity.ToTable("geothermal_parameters");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Getrequest).HasColumnName("getrequest");
            entity.Property(e => e.LastPing)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("last_ping");
            entity.Property(e => e.LastUpdate)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("last_update");
            entity.Property(e => e.Srid).HasColumnName("srid");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    private partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
