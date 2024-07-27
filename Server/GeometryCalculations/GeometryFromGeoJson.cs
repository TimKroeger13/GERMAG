using NetTopologySuite.IO;
using ProjNet.CoordinateSystems.Transformations;
using ProjNet.CoordinateSystems;
using NetTopologySuite.Geometries;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;
using System.Text.Json;
using Newtonsoft.Json;
using GERMAG.DataModel.Database;

namespace GERMAG.Server.GeometryCalculations;

public interface IGeometryFromGeoJson
{
    Task<int> GetGeometryFromgeoJson(string Geojson, int srid);
}

public class GeometryFromGeoJson : IGeometryFromGeoJson
{
    public async Task<int> GetGeometryFromgeoJson(string Geojson, int srid)
    {
        var sridSource = srid;
        var sridTarget = 25833;

        var serializer = GeoJsonSerializer.Create();

        // Deserialize the GeoJSON string to a Feature
        NetTopologySuite.Features.Feature? feature;
        using (var stringReader = new System.IO.StringReader(Geojson))
        using (var jsonReader = new JsonTextReader(stringReader))
        {
            feature = serializer.Deserialize<NetTopologySuite.Features.Feature>(jsonReader);
        }

        // Get the Geometry from the Feature
        NetTopologySuite.Geometries.Geometry? geometry = feature.Geometry;




        // Further processing...


        /*
        var sridSource = request.Srid;
        var sridTarget = 25833; // Target SRID

        // Deserialize GeoJSON to NetTopologySuite Geometry
        var geoJsonReader = new GeoJsonReader();
        var geometry = geoJsonReader.Read<Geometry>(request.Geojson.ToString());

        // Define the source and target coordinate reference systems
        var csFactory = new CoordinateSystemFactory();
        var sourceCRS = csFactory.CreateFromEpsgCode(sridSource);
        var targetCRS = csFactory.CreateFromEpsgCode(sridTarget);

        // Create the coordinate transformation
        var transform = new CoordinateTransformationFactory().CreateFromCoordinateSystems(sourceCRS, targetCRS);

        // Transform the geometry
        var transformedGeometry = TransformGeometry(geometry, transform);

        // Serialize the transformed geometry back to GeoJSON
        var geoJsonWriter = new GeoJsonWriter();
        var transformedGeoJson = geoJsonWriter.Write(transformedGeometry);
        */


        return 1;
    }

    private NetTopologySuite.Geometries.Geometry TransformGeometry(NetTopologySuite.Geometries.Geometry geometry, ICoordinateTransformation transform)
    {
        NetTopologySuite.Geometries.Coordinate[] TransformCoordinates(NetTopologySuite.Geometries.Coordinate[] coordinates)
        {
            return coordinates
                .Select(coord =>
                {
                    var result = transform.MathTransform.Transform(coord.X, coord.Y);
                    return new NetTopologySuite.Geometries.Coordinate(result.x, result.y);
                })
                .ToArray();
        }

        if (geometry is Polygon polygon)
        {
            // Transform exterior and interior rings of a Polygon
            var transformedExteriorRing = TransformCoordinates(polygon.ExteriorRing.Coordinates);
            var transformedInteriorRings = polygon.InteriorRings
                .Select(ring => TransformCoordinates(ring.Coordinates))
                .ToArray();

            var newExteriorRing = new LinearRing(transformedExteriorRing);
            var newInteriorRings = transformedInteriorRings.Select(ring => new LinearRing(ring)).ToArray();
            return new Polygon(newExteriorRing, newInteriorRings);
        }
        else if (geometry is MultiPolygon multiPolygon)
        {
            // Transform each Polygon within a MultiPolygon
            var transformedPolygons = multiPolygon.Geometries
                .Cast<Polygon>()
                .Select(polygon => TransformGeometry(polygon, transform))
                .ToArray();

            return new MultiPolygon(transformedPolygons.Cast<Polygon>().ToArray());
        }
        else
        {
            throw new NotSupportedException($"Geometry type {geometry.GetType().Name} is not supported.");
        }
    }
}

