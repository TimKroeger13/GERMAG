using GERMAG.Shared.PointProperties;
using GERMAG.Shared;
using GERMAG.DataModel.Database;
using GERMAG.Server.ReportCreation;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace GERMAG.Server.GeometryCalculations;

public interface IGetProbeSepcificDataSingleProbe
{
    Task<ProbePoint?> GetSingleProbeData(LandParcel landParcelElement, ProbePoint? SingleProbePoint, DataContext context);
}

public class GetProbeSepcificDataSingleProbe(IParameterDeserialator parameterDeserialator, IRating rating) : IGetProbeSepcificDataSingleProbe
{
    private Regex protectionRex = new Regex("schutz", RegexOptions.IgnoreCase);
    public async Task<ProbePoint?> GetSingleProbeData(LandParcel landParcelElement, ProbePoint? SingleProbePoint, DataContext context)
    {

        if (SingleProbePoint == null || SingleProbePoint.Geometry == null)
        {
            return null;
        }

        SingleProbePoint.Geometry.SRID = 25833;

        await using var transaction = context.Database.BeginTransaction();

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

        context.SaveChanges();
        transaction.Commit();



        var RestrictionText = intersectingResult.Find(element => element.Type == TypeOfData.geo_poten_restrict);

        bool IsProtectedBool = false;
        if (RestrictionText != null)
        {
            var DeserializedRestrictionText = await Task.Run(() => parameterDeserialator.DeserializeParameters(RestrictionText?.Parameter ?? ""));
            IsProtectedBool = protectionRex.IsMatch(DeserializedRestrictionText.Text ?? string.Empty);
        }
       
        //var ThermalConductivityValue = intersectingResult.Find(element => element.Type == TypeOfData.th)














        var UnserilizedDepthRestrictions = intersectingResult.Find(element => element.Type == TypeOfData.depth_restrictions);
        double? MaxDepth;

        if (UnserilizedDepthRestrictions == null)
        {
            MaxDepth = 100; //Berlin spesific
        }
        else
        {
            var DeserializedDepthRestrictions = await Task.Run(() => parameterDeserialator.DeserializeParameters(UnserilizedDepthRestrictions?.Parameter ?? ""));
            MaxDepth = DeserializedDepthRestrictions.VALUE;
        }

        //Poetential = 100,80,60,40
        List<int> PotentialDepth = new() { 100, 80, 60, 40, 0 }; //Berlin spesific

        int GeoPotenDepth = PotentialDepth.Find(value => value <= MaxDepth);

        double? GeoPoten = null;
        double? ThermalCon = null;

        if (GeoPotenDepth >= 100)
        {
            GeoPoten = GetValue(intersectingResult, TypeOfData.geo_poten_100m_with_2400ha, "La_100txt");
            ThermalCon = GetValue(intersectingResult, TypeOfData.thermal_con_100, "La_100txt");
        }
        else if (GeoPotenDepth >= 80)
        {
            GeoPoten = GetValue(intersectingResult, TypeOfData.geo_poten_80m_with_2400ha, "La_80txt");
            ThermalCon = GetValue(intersectingResult, TypeOfData.thermal_con_80, "La_80txt");
        }
        else if (GeoPotenDepth >= 60)
        {
            GeoPoten = GetValue(intersectingResult, TypeOfData.geo_poten_60m_with_2400ha, "La_60txt");
            ThermalCon = GetValue(intersectingResult, TypeOfData.thermal_con_60, "La_60txt");
        }
        else if (GeoPotenDepth >= 40)
        {
            GeoPoten = GetValue(intersectingResult, TypeOfData.geo_poten_40m_with_2400ha, "La_40txt");
            ThermalCon = GetValue(intersectingResult, TypeOfData.thermal_con_40, "La_40txt");
        }

        if(SingleProbePoint.Properties == null)
        {
            var CorrectedProbpoint = new ProbePoint
            {
                Geometry = SingleProbePoint.Geometry,
                Properties = new Shared.PointProperties.Properties { GeoPoten = null, MaxDepth = null, GeoPotenDepth = null }
            };

            SingleProbePoint = CorrectedProbpoint;
        }

        //Water Protection
        //Conductfity
        //Tempeture

        SingleProbePoint.Properties.MaxDepth = MaxDepth;
        SingleProbePoint.Properties.GeoPotenDepth = GeoPotenDepth;
        SingleProbePoint.Properties.GeoPoten = GeoPoten;
        SingleProbePoint.Properties.RawExtractionKW = GeoPoten * MaxDepth * 2400 / 1000;

        var ratingFactor = await rating.CalculateRating(MaxDepth, ThermalCon, 12.8, IsProtectedBool);

        return SingleProbePoint;
    }

    private double? GetValue(List<GeometryElementParameter> intersectingResult, TypeOfData typeOfData, String ParameterName)
    {
        GeometryElementParameter? UnserilizedGeoPoten = intersectingResult.Find(element => element.Type == typeOfData);

        var propertyInfo = typeof(Shared.Properties).GetProperty(ParameterName);
        if (propertyInfo == null) { return null; }

        if (UnserilizedGeoPoten == null) { return null;  }

        Shared.Properties DeserializedGeoPoten = parameterDeserialator.DeserializeParameters(UnserilizedGeoPoten?.Parameter ?? "");

        string? propertyValue = propertyInfo.GetValue(DeserializedGeoPoten) as string;
        double? GeoPoten = ParseStringToValue(propertyValue ?? string.Empty);

        return GeoPoten;
    }

    private double? ParseStringToValue(string valueRange)
    {
        List<double> numbers = new();

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
            return null;
        }

        double minValue = numbers.Min();
        double maxValue = numbers.Max();

        return (minValue + maxValue) / 2.0;
    }
}
