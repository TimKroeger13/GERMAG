using GERMAG.DataModel.Database;
using GERMAG.Shared;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;

namespace GERMAG.Server.GeometryCalculations;

public interface IRestrictionFromLandParcel
{
    Task<List<Report>> CalculateRestrictions(LandParcel landParcelElement, List<Report> report);
}

public class RestrictionFromLandParcel(DataContext context) : IRestrictionFromLandParcel
{
    public async Task<List<Report>> CalculateRestrictions(LandParcel landParcelElement, List<Report> report)
    {
        return await Task.Run(() =>
        {
            var geoJsonWriter = new GeoJsonWriter();

            var buildingID = context.GeothermalParameter.First(gp => gp.Type == TypeOfData.building_surfaces).Id;

            var buldingIntersection = context.GeoData.Where(gd => gd.ParameterKey == buildingID && gd.Geom!.Intersects(landParcelElement.Geometry)).Select(gd => new { gd.Geom });

            NetTopologySuite.Geometries.Geometry? mergedBuildings = new GeometryFactory().BuildGeometry(buldingIntersection.Select(item => item.Geom)).Union();

            Polygon? landParcelPolygon = (Polygon?)landParcelElement.Geometry;

            LineString? landParcelLineString = landParcelPolygon?.ExteriorRing;

            NetTopologySuite.Geometries.Geometry? bufferedLandParcel = landParcelLineString?.Buffer(OfficalParameters.LandParcelDistance);

            if (mergedBuildings == null || mergedBuildings.IsEmpty)
            {
                GeometryFactory geometryFactory = new GeometryFactory();
                mergedBuildings = geometryFactory.CreatePolygon();
            }

            NetTopologySuite.Geometries.Geometry? bufferedBuldings = mergedBuildings?.Buffer(OfficalParameters.BuildingDistance);

            //NetTopologySuite.Geometries.Geometry? UsableArea = landParcelPolygon?.Difference(bufferedLandParcel).Difference(bufferedBuldings);
            //UsableArea = UsableArea?.Union();

            NetTopologySuite.Geometries.Geometry? UsableArea;


            if (bufferedBuldings is Polygon bufferedBuldingsPolygon)
            {
                // UsableArea is a Polygon, directly calculate the difference
                UsableArea = landParcelPolygon?.Difference(bufferedLandParcel).Difference(bufferedBuldings);
                UsableArea = UsableArea?.Union();
            }
            else if (bufferedBuldings is MultiPolygon bufferedBuldingsMultipolygon)
            {
                UsableArea = landParcelPolygon?.Difference(bufferedLandParcel);
                foreach (var polygon in bufferedBuldingsMultipolygon.Geometries.OfType<Polygon>())
                {
                    UsableArea = UsableArea?.Difference(polygon);
                }
            }
            else
            {
                // Handle other cases as needed
                UsableArea = null;
            }

            //NetTopologySuite.Geometries.Geometry? RestictionArea = landParcelPolygon?.Difference(UsableArea);

            NetTopologySuite.Geometries.Geometry? RestictionArea;

            if (UsableArea is Polygon usablePolygon)
            {
                // UsableArea is a Polygon, directly calculate the difference
                RestictionArea = landParcelPolygon?.Difference(usablePolygon);
            }
            else if (UsableArea is MultiPolygon usableMultiPolygon)
            {
                // UsableArea is a MultiPolygon, initialize RestictionArea to the original landParcelPolygon
                RestictionArea = landParcelPolygon;

                // Iterate over MultiPolygon components and calculate the difference for each
                foreach (var polygon in usableMultiPolygon.Geometries.OfType<Polygon>())
                {
                    RestictionArea = RestictionArea?.Difference(polygon);
                }
            }
            else
            {
                // Handle other cases as needed
                RestictionArea = null;
            }


            //RestictionArea = landParcelPolygon?.Intersection(RestictionArea);
            //RestictionArea = RestictionArea?.Union();


            report[0].Geometry_Usable = geoJsonWriter.Write(UsableArea);
            report[0].Geometry_Restiction = geoJsonWriter.Write(RestictionArea);
            report[0].Geometry_Usable_Area = UsableArea?.Area ?? 0;
            report[0].Geometry_Restiction_Area = RestictionArea?.Area ?? 0;

            return report;
        });
    }
}