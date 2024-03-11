using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GERMAG.Shared;

public class RestricionRoot
{
    public NetTopologySuite.Geometries.Geometry? Geometry_Usable { get; set; }
    public NetTopologySuite.Geometries.Geometry? Geometry_Restiction { get; set; }
    public String? Geometry_Usable_geoJson { get; set; }
    public String? Geometry_Restiction_geoJson { get; set; }
    public double? Usable_Area { get; set; }
    public double? Restiction_Area { get; set; }
}



/*

public class LandParcel
{
    public int? GeoDataID { get; set; }
    public int? ParameterKey { get; set; }
    public string? Parameter { get; set; }
    public NetTopologySuite.Geometries.Geometry? Geometry { get; set; }
    public string? GeometryJson { get; set; }
    public bool? Error { get; set; }
}
*/
