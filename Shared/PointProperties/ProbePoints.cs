using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GERMAG.Shared.PointProperties;

public class ProbePoint
{
    public string Type => "Feature";
    public string? Geometry { get; set; }
    public Properties? Properties { get; set; }
}

public class Properties
{
    public double? GeoPoten { get; set; }
    public double? ThermalCon { get; set; }
}
