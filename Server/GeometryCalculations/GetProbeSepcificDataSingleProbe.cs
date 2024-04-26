using GERMAG.Shared.PointProperties;
using GERMAG.Shared;
using GERMAG.DataModel.Database;
using GERMAG.Server.ReportCreation;

namespace GERMAG.Server.GeometryCalculations;

public interface IGetProbeSepcificDataSingleProbe
{
    Task<ProbePoint?> GetSingleProbeData(LandParcel landParcelElement, ProbePoint? SingleProbePoint, DataContext context);
}

public class GetProbeSepcificDataSingleProbe(IParameterDeserialator parameterDeserialator) : IGetProbeSepcificDataSingleProbe
{
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

        if (GeoPotenDepth >= 100)
        {
            GeoPoten = GetPotential(intersectingResult, TypeOfData.geo_poten_100m_with_2400ha);
        }
        else if (GeoPotenDepth >= 80)
        {
            GeoPoten = GetPotential(intersectingResult, TypeOfData.geo_poten_100m_with_2400ha);
        }
        else if (GeoPotenDepth >= 60)
        {
            GeoPoten = GetPotential(intersectingResult, TypeOfData.geo_poten_100m_with_2400ha);
        }
        else if (GeoPotenDepth >= 40)
        {
            GeoPoten = GetPotential(intersectingResult, TypeOfData.geo_poten_100m_with_2400ha);
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

        SingleProbePoint.Properties.MaxDepth = MaxDepth;
        SingleProbePoint.Properties.GeoPotenDepth = GeoPotenDepth;
        SingleProbePoint.Properties.GeoPoten = GeoPoten;

        return SingleProbePoint;
    }

    private double? GetPotential(List<GeometryElementParameter> intersectingResult, TypeOfData typeOfData)
    {
        GeometryElementParameter? UnserilizedGeoPoten = intersectingResult.Find(element => element.Type == typeOfData);

        if(UnserilizedGeoPoten == null) { return null;  }

        Shared.Properties DeserializedGeoPoten = parameterDeserialator.DeserializeParameters(UnserilizedGeoPoten?.Parameter ?? "");
        double? GeoPoten = ParseStringToValue(DeserializedGeoPoten.La_100txt ?? "");

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
            throw new Exception("ParseStringToValue: Numbers could not get paresed");
        }

        double minValue = numbers.Min();
        double maxValue = numbers.Max();

        return (minValue + maxValue) / 2.0;
    }
}
