using GERMAG.DataModel.Database;
using GERMAG.Shared;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using NetTopologySuite.Operation.Overlay;

namespace GERMAG.Server.GeometryCalculations;

public interface IRestrictionFromLandParcel
{
    Task<Restricion> CalculateRestrictions(LandParcel landParcelElement);
}

public class RestrictionFromLandParcel(DataContext context) : IRestrictionFromLandParcel
{
    public async Task<Restricion> CalculateRestrictions(LandParcel landParcelElement)
    {
        return await Task.Run(() =>
        {
            var geoJsonWriter = new GeoJsonWriter();

            var buildingID = context.GeothermalParameter.First(gp => gp.Type == TypeOfData.building_surfaces).Id;

            var buldingIntersection = context.GeoData.Where(gd => gd.ParameterKey == buildingID && gd.Geom!.Intersects(landParcelElement.Geometry)).Select(gd => new { gd.Geom });

            NetTopologySuite.Geometries.Geometry? mergedBuildings = new GeometryFactory().BuildGeometry(buldingIntersection.Select(item => item.Geom)).Union();

            Polygon? landParcelPolygon = (Polygon?)landParcelElement.Geometry;

            LineString? landParcelLineString = landParcelPolygon?.ExteriorRing;

            NetTopologySuite.Geometries.Geometry? bufferedLandParcel = landParcelLineString?.Buffer(OfficalParameters.LandParcelDistance + (OfficalParameters.ProbeDiameter / 2));

            if (mergedBuildings?.IsEmpty != false)
            {
                GeometryFactory geometryFactory = new GeometryFactory();
                mergedBuildings = geometryFactory.CreatePolygon();
            }

            NetTopologySuite.Geometries.Geometry? bufferedBuldings = mergedBuildings?.Buffer(OfficalParameters.BuildingDistance + (OfficalParameters.ProbeDiameter / 2));

            NetTopologySuite.Geometries.Geometry? UsableArea = landParcelPolygon?.Difference(bufferedLandParcel).Difference(bufferedBuldings);
            UsableArea = UsableArea?.Union();

            NetTopologySuite.Geometries.Geometry? RestictionArea = bufferedLandParcel?.Union(bufferedBuldings);
            RestictionArea = landParcelPolygon?.Intersection(RestictionArea);
            RestictionArea = RestictionArea?.Union();

            var returnValue = new Restricion
            {
                Geometry_Usable = UsableArea,
                Geometry_Restiction = RestictionArea,
                Geometry_Usable_geoJson = geoJsonWriter.Write(UsableArea),
                Geometry_Restiction_geoJson = geoJsonWriter.Write(RestictionArea),
                Usable_Area = UsableArea?.Area ?? 0,
                Restiction_Area = RestictionArea?.Area ?? 0,
            };

            return returnValue;
        });
    }
}