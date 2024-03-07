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
            GeometryFactory geometryFactory = new GeometryFactory();

            var geoJsonWriter = new GeoJsonWriter();

            var buildingID = context.GeothermalParameter.First(gp => gp.Type == TypeOfData.building_surfaces).Id;

            var buldingIntersection = context.GeoData.Where(gd => gd.ParameterKey == buildingID && gd.Geom!.Intersects(landParcelElement.Geometry)).Select(gd => new { gd.Geom });

            NetTopologySuite.Geometries.Geometry? mergedBuildings = new GeometryFactory().BuildGeometry(buldingIntersection.Select(item => item.Geom)).Union();

            Polygon? landParcelPolygon = (Polygon?)landParcelElement.Geometry;

            LineString? landParcelLineString = landParcelPolygon?.ExteriorRing;

            NetTopologySuite.Geometries.Geometry? bufferedLandParcel = landParcelLineString?.Buffer(OfficalParameters.LandParcelDistance);

            if (mergedBuildings == null || mergedBuildings.IsEmpty)
            {
                mergedBuildings = geometryFactory.CreatePolygon();
            }

            NetTopologySuite.Geometries.Geometry? bufferedBuldings = mergedBuildings?.Buffer(OfficalParameters.BuildingDistance);

            //NetTopologySuite.Geometries.Geometry? UsableArea = landParcelPolygon?.Difference(bufferedLandParcel).Difference(bufferedBuldings);
            //UsableArea = UsableArea?.Union();

            NetTopologySuite.Geometries.Geometry? UsableArea;
            //NetTopologySuite.Geometries.MultiPolygon UsableArea = NetTopologySuite.Geometries.MultiPolygon.Create();



            if (bufferedBuldings is Polygon bufferedBuldingsPolygon)
            {
                // UsableArea is a Polygon, directly calculate the difference
                UsableArea = landParcelPolygon?.Difference(bufferedLandParcel).Difference(bufferedBuldings);
                UsableArea = UsableArea?.Union();
            }
            else if (bufferedBuldings is MultiPolygon bufferedBuldingsMultipolygon)
            {
                UsableArea = landParcelPolygon?.Difference(bufferedLandParcel);

                //UsableArea = UsableArea?.Difference(bufferedBuldingsMultipolygon[0]);
                //UsableArea = UsableArea?.Difference(bufferedBuldingsMultipolygon[1]);

                NetTopologySuite.Geometries.MultiPolygon? a = (MultiPolygon?)UsableArea?.Difference(bufferedBuldingsMultipolygon[0]);
                NetTopologySuite.Geometries.MultiPolygon? b = (MultiPolygon?)a?.Difference(bufferedBuldingsMultipolygon[1]);

                UsableArea = b;

                //UsableArea.Geometries.Add(UsableArea?.Difference(bufferedBuldingsMultipolygon[0]));
                //UsableArea.Geometries.Add(UsableArea?.Difference(bufferedBuldingsMultipolygon[1]));



                //var buildingSubtraction = bufferedBuldingsMultipolygon[0].Union(bufferedBuldingsMultipolygon[1]);
                //UsableArea = UsableArea?.Difference(buildingSubtraction);

                //UsableArea = UsableArea?.Difference(bufferedBuldingsMultipolygon);

                /*                UsableArea = bufferedBuldingsMultipolygon.Geometries
                                    .OfType<Polygon>()
                                    .Aggregate(UsableArea, (currentUsableArea, polygon) =>
                                        currentUsableArea?.Difference(polygon)) ?? UsableArea;*/

                /*                foreach (var polygon in bufferedBuldingsMultipolygon.Geometries.OfType<Polygon>())
                                {
                                    UsableArea = UsableArea?.Difference(polygon);
                                }*/
            }
            else
            {
                UsableArea = null;
            }
            //UsableArea = UsableArea?.Union();


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
/*                foreach (var polygon in usableMultiPolygon.Geometries.OfType<Polygon>())
                {
                    RestictionArea = RestictionArea?.Difference(polygon);
                }*/

                foreach (var polygon in usableMultiPolygon.Geometries.OfType<Polygon>())
                {
                    if (polygon.IsValid)
                    {
                        RestictionArea = RestictionArea?.Difference(polygon);
                    }
                }

                RestictionArea = RestictionArea?.Union();
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