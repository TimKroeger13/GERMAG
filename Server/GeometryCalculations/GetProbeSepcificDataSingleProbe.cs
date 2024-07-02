using GERMAG.Shared.PointProperties;
using GERMAG.Shared;
using GERMAG.DataModel.Database;
using GERMAG.Server.ReportCreation;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Globalization;

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
        double? Temperature = null;

        if (GeoPotenDepth >= 100)
        {
            GeoPoten = GetValue(intersectingResult, TypeOfData.geo_poten_100m_with_2400ha, "La_100txt");
            ThermalCon = GetValue(intersectingResult, TypeOfData.thermal_con_100, "La_100txt");
            Temperature = GetValue(intersectingResult, TypeOfData.mean_water_temp_20to100, "Grwtemp_text");
        }
        else if (GeoPotenDepth >= 80)
        {
            GeoPoten = GetValue(intersectingResult, TypeOfData.geo_poten_80m_with_2400ha, "La_80txt");
            ThermalCon = GetValue(intersectingResult, TypeOfData.thermal_con_80, "La_80txt");
            Temperature = GetValue(intersectingResult, TypeOfData.mean_water_temp_20to100, "Grwtemp_text");
        }
        else if (GeoPotenDepth >= 60)
        {
            GeoPoten = GetValue(intersectingResult, TypeOfData.geo_poten_60m_with_2400ha, "La_60txt");
            ThermalCon = GetValue(intersectingResult, TypeOfData.thermal_con_60, "La_60txt");
            Temperature = GetValue(intersectingResult, TypeOfData.mean_water_temp_60, "Grwtemp_text");
        }
        else{
            GeoPoten = GetValue(intersectingResult, TypeOfData.geo_poten_40m_with_2400ha, "La_40txt");
            ThermalCon = GetValue(intersectingResult, TypeOfData.thermal_con_40, "La_40txt");
            Temperature = GetValue(intersectingResult, TypeOfData.mean_water_temp_40, "Grwtemp_text");
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

        var ratingFactor = await rating.CalculateRating(MaxDepth, ThermalCon, Temperature, IsProtectedBool);

        SingleProbePoint.Properties.MaxDepth = MaxDepth;
        SingleProbePoint.Properties.GeoPotenDepth = GeoPotenDepth;
        SingleProbePoint.Properties.GeoPoten = GeoPoten;
        SingleProbePoint.Properties.RawExtractionKW = GeoPoten * MaxDepth * 2400 / 1000;

        SingleProbePoint.Properties.Rating = ratingFactor;

        return SingleProbePoint;
    }

    private double? GetValue(List<GeometryElementParameter> intersectingResult, TypeOfData typeOfData, String ParameterName)
    {
        var UnserilizedParameter = intersectingResult.Where(element => element.Type == typeOfData).ToList();

        if (UnserilizedParameter == null) { return null; }

        var propertyInfo = typeof(Shared.Properties).GetProperty(ParameterName);
        if (propertyInfo == null) { return null; }

        double? HeighestValue = double.NegativeInfinity;

        foreach (var SingleUnParameter in UnserilizedParameter)
        {
            Shared.Properties DeserializedGeoPoten = parameterDeserialator.DeserializeParameters(SingleUnParameter?.Parameter ?? "");

            string? propertyValue = propertyInfo.GetValue(DeserializedGeoPoten) as string;
            double? CurrentValue = ParseStringToValue(propertyValue ?? string.Empty);

            if(HeighestValue < CurrentValue) { HeighestValue = CurrentValue; }
        }

        return HeighestValue;
    }

    /*
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
    }*/

    private double? ParseStringToValue(string valueRange)
    {
        // Replace commas with dots
        valueRange = CommaToDotRegex().Replace(valueRange, ".");
        valueRange = CutRegex().Replace(valueRange, "-");
        valueRange = RemoveLessThanRegex().Replace(valueRange, "");
        valueRange = RemoveGreaterThanRegex().Replace(valueRange, "");

        // Split the string by "bis"
        string[] parts = valueRange.Split('-', '>');

        List<double> numbers = new();

        foreach (string part in parts)
        {
            if (double.TryParse(part.Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out double number))
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

    private static Regex CutRegex() => new Regex("bis", RegexOptions.Compiled);
    private static Regex CommaToDotRegex() => new Regex(",", RegexOptions.Compiled);
    private static Regex RemoveLessThanRegex() => new Regex("<", RegexOptions.Compiled);
    private static Regex RemoveGreaterThanRegex() => new Regex(">", RegexOptions.Compiled);
}
