using Microsoft.EntityFrameworkCore.Storage.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GERMAG.Shared;

public static class OfficalParameters
{
    public static double ProbeDiameter { get; } = 0; // meter (radius)
    public static double TreeDistance { get; } = 4; // meter (radius)
    public static double LandParcelDistance { get; } = 3; // meter 0 für die AUswertung
    public static double BuildingDistance { get; } = 2; // meter
    public static double ProbeDistance { get; } = 6; //meter (radius)
    public static int MaximalAreaSizeForCalculations { get; } = 100000; //sqaure meters

    public static double DepthFactorRatio { get; } = (double)24.74 / (double)100;
    public static double ThermalConFactorRatio { get; } = (double)18.26 / (double)100;
    public static double UnderGroundTempFactorRatio { get; } = (double)57 / (double)100;

    public static double DepthFactorMin { get; } = 30;
    public static double DepthFactorMax { get; } = 100;
    public static double ThermalConFactorMin { get; } = 1.6;
    public static double ThermalConFactorMax { get; } = 2.8;
    public static double UnderGroundTempMin { get; } = 8;
    public static double UnderGroundTempMax { get; } = 13;
}
