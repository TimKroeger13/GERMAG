using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GERMAG.Shared.PointProperties;

public class ProbePoint
{
    public string Type => "Feature";
    public NetTopologySuite.Geometries.Geometry? Geometry { get; set; }
    public string? GeometryJson { get; set; }
    public Properties? Properties { get; set; }
}

public class Properties
{
    public double? GeoPoten2400 { get; set; }
    public double? MaxDepth { get; set; }
    public double? GeoPotenDepth { get; set; }
}
