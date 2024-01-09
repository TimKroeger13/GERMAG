using NetTopologySuite.Geometries;

namespace GERMAG.DataModel.Database;

public partial class GeoDatum
{
    public GeoDatum()
    {
    }

    public int Id { get; set; }
    public int ParameterKey { get; set; }
    public Geometry? Geom { get; set; }
    public virtual GeothermalParameter ParameterKeyNavigation { get; set; } = null!;
}
