using NetTopologySuite.Geometries;
using GERMAG.DataModel.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GERMAG.Shared;

public class LandParcel
{
    public int? ParameterKey { get; set; }
    public NetTopologySuite.Geometries.Geometry? Geometry { get; set; }
    public string? GeometryJson { get; set; }
}
