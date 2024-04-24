using NetTopologySuite.Geometries;

namespace GERMAG.DataModel.Database;

public partial class AxSelectedTest
{
    public AxSelectedTest()
    {
    }

    public Geometry? Geom { get; set; }
}
