using GERMAG.DataModel.Database;
using GERMAG.Shared;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;

namespace GERMAG.Server.GeometryCalculations;

public interface IRestrictionFromLandParcel
{
    Task<String> CalculateRestrictions(LandParcel landParcelElement);
}

public class RestrictionFromLandParcel(DataContext context) : IRestrictionFromLandParcel
{
    public async Task<String> CalculateRestrictions(LandParcel landParcelElement)
    {
        return await Task.Run(() =>
        {

            var buildingID = context.GeothermalParameter.First(gp => gp.Type == TypeOfData.building_surfaces).Id;

            var buldingIntersection = context.GeoData.Where(gd => gd.ParameterKey == buildingID && gd.Geom!.Intersects(landParcelElement.Geometry)).Select(gd => new { gd.Geom });

            var mergedBuildings = new GeometryFactory().BuildGeometry(buldingIntersection.Select(item => item.Geom)).Union();




            Polygon? landParcelPolygon = (Polygon?)landParcelElement.Geometry;

            LineString? landParcelLineString = landParcelPolygon?.ExteriorRing;

          NetTopologySuite.Geometries.Geometry? bufferedLandParcel = landParcelLineString?.Buffer(OfficalParameters.LandParcelDistance);

          NetTopologySuite.Geometries.Geometry? bufferedBuldings = mergedBuildings?.Buffer(OfficalParameters.BuildingDistance);


            NetTopologySuite.Geometries.Geometry? difference = landParcelPolygon?.Difference(bufferedLandParcel).Difference(mergedBuildings); //.Difference(buldingIntersection);

            NetTopologySuite.Geometries.Geometry? intersection = landParcelPolygon?.Intersection(bufferedLandParcel).Union(mergedBuildings);


            var b = 3;

            /*            var IntersectingGeometry = context.GeoData
                .Where(gd => gd.ParameterKey != landParcelElement.ParameterKey && gd.Geom!.Intersects(landParcelElement.Geometry))
                .Select(gd => new
                {
                    gd.ParameterKey,
                    gd.Parameter
                });

                        landParcelElement.Geometry;*/


            return "Working dependancy Injection";
        });
    }
}
