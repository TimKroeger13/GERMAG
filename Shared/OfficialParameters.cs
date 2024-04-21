using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GERMAG.Shared;

public static class OfficalParameters
{
    public static double ProbeDiameter { get; } = 0.2; // meter (radius)
    public static double TreeDistance { get; } = 4; // meter (radius)
    public static double LandParcelDistance { get; } = 3; // meter 0 für die AUswertung
    public static double BuildingDistance { get; } = 2; // meter
    public static double ProbeDistance { get; } = 6; //meter (radius)
    public static int MaximalAreaSizeForCalculations { get; } = 100000; //sqaure meters
}
