﻿using GERMAG.DataModel.Database;
using GERMAG.Server.ReportCreation;
using GERMAG.Shared;
using GERMAG.Shared.PointProperties;
using Newtonsoft.Json.Linq;

namespace GERMAG.Server.GeometryCalculations;

public interface IGetProbeSpecificData
{
    Task<List<ProbePoint?>> GetPointProbeData(LandParcel landParcelElement, List<ProbePoint?> probePoints);
}

public class GetProbeSpecificData(DataContext context, IParameterDeserialator parameterDeserialator) : IGetProbeSpecificData
{
    public async Task<List<ProbePoint?>> GetPointProbeData(LandParcel landParcelElement, List<ProbePoint?> probePoints)
    {
        foreach (var SingleProbePoint in probePoints)
        {
            if (SingleProbePoint == null || SingleProbePoint.Geometry == null || SingleProbePoint.Properties == null)
            {
                continue;
            }

            SingleProbePoint.Geometry.SRID = 25833;

            var IntersectingGeometry = context.GeoData
                .Where(gd => gd.ParameterKey != landParcelElement.ParameterKey && gd.Geom!.Intersects(SingleProbePoint.Geometry))
                .Select(gd => new
                {
                    gd.ParameterKey,
                    gd.Parameter
                });

            var intersectingResult = IntersectingGeometry
                .Join(
                context.GeothermalParameter,
                ig => ig.ParameterKey,
                gp => gp.Id,
                (ig, gp) => new GeometryElementParameter
                {
                    Type = gp.Type,
                    ParameterKey = ig.ParameterKey,
                    Parameter = ig.Parameter
                }).ToList();

            var UnserilizedDepthRestrictions = intersectingResult.FirstOrDefault(element => element.Type == TypeOfData.depth_restrictions);

            double? MaxDepth;

            if (UnserilizedDepthRestrictions == null)
            {
                MaxDepth = 100; //Berlin spesific
            }
            else
            {
                var DeserializedDepthRestrictions = parameterDeserialator.DeserializeParameters(UnserilizedDepthRestrictions?.Parameter ?? "");
                MaxDepth = DeserializedDepthRestrictions.VALUE;

            }

            //Poetential = 100,80,60,40
            List<int> PotentialDepth = new List<int> { 100, 80, 60, 40, 0}; //Berlin spesific

            int GeoPotenDepth = PotentialDepth.FirstOrDefault(value => value <= MaxDepth);

            double? GeoPoten = null;

            if (GeoPotenDepth >= 100)
            {
                GeoPoten = GetPotential(intersectingResult, TypeOfData.geo_poten_100m_with_2400ha, "La_100txt");
            }
            else if(GeoPotenDepth >= 80)
            {
                GeoPoten = GetPotential(intersectingResult, TypeOfData.geo_poten_100m_with_2400ha, "La_80txt");
            }
            else if (GeoPotenDepth >= 60)
            {
                GeoPoten = GetPotential(intersectingResult, TypeOfData.geo_poten_100m_with_2400ha, "La_60txt");
            }
            else if (GeoPotenDepth >= 40)
            {
                GeoPoten = GetPotential(intersectingResult, TypeOfData.geo_poten_100m_with_2400ha, "La_40txt");
            }

            SingleProbePoint.Properties.MaxDepth = MaxDepth;
            SingleProbePoint.Properties.GeoPotenDepth = GeoPotenDepth;
            SingleProbePoint.Properties.GeoPoten = GeoPoten;


        }

        var a = probePoints.ToList();

        return probePoints;
    }

    private double? GetPotential (List<GeometryElementParameter> intersectingResult, TypeOfData typeOfData, string ParameterKeyValue)
    {
        GeometryElementParameter? UnserilizedGeoPoten = intersectingResult.FirstOrDefault(element => element.Type == typeOfData);
        Shared.Properties DeserializedGeoPoten = parameterDeserialator.DeserializeParameters(UnserilizedGeoPoten?.Parameter ?? "");
        double? GeoPoten = ParseStringToValue(DeserializedGeoPoten.La_100txt ?? "");

        return GeoPoten;
    }


    private double? ParseStringToValue(string valueRange)
    {
        List<double> numbers = new List<double>();

        string[] parts = valueRange.Split('-', '>');

        foreach (string part in parts)
        {
            if (double.TryParse(part.Trim(), out double number))
            {
                numbers.Add(number);
            }
        }

        if (numbers.Count == 0)
        {
            throw new Exception("ParseStringToValue: Numbers could not get paresed");
        }

        double minValue = numbers.Min();
        double maxValue = numbers.Max();

        return (minValue + maxValue) / 2.0;
    }
}