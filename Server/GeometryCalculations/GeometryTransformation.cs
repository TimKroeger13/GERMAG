using NetTopologySuite.Geometries;
using System;
using NetTopologySuite.Features;
using NetTopologySuite.IO;
using NetTopologySuite.CoordinateSystems.Transformations;
using Newtonsoft.Json;
using ProjNet.CoordinateSystems;
using ProjNet.CoordinateSystems.Transformations;
using GeoAPI.Geometries;
using GeoAPI.CoordinateSystems;

namespace GERMAG.Server.GeometryCalculations;

public interface IGeometryTransformation
{
    Task<int> TransformGeometry(Geometry geometry, int srid);
}

public class GeometryTransformation : IGeometryTransformation
{
    public async Task<int> TransformGeometry(NetTopologySuite.Geometries.Geometry geometry, int SourceSrid)
    {
        var TagedSrid = 25833;

        if (geometry is NetTopologySuite.Geometries.Polygon)
        {

            for (int i = 0; i < geometry.Coordinates.Count(); i++)
            {
                var RawCoordinates = geometry.Coordinates[i];

                var transformedCoordinate = await transformCoordinates(RawCoordinates[0], RawCoordinates[1], SourceSrid, TagedSrid) ?? throw new Exception("Coordiantes could not be transformed");
                geometry.Coordinates[i][0] = transformedCoordinate[0];
            }


            //geometry.Coordinates.Count()
            //geometry.Coordinates[0].CoordinateValue[0]  geometry.Coordinates[0].CoordinateValue[1]

            var b = 3;
        }





            

        var coordinate = (x: 13.407990038394928, y: 52.52984737034732);

        //var transformedCoordinate = transformCoordinates(coordinate.x, coordinate.y, SourceSrid, TagedSrid);

        return 1;
    }



    async private Task<double[]?> transformCoordinates (double lon, double lat, int SourceSrid, int TagedSrid)
    {
        if (SourceSrid == 4326 && TagedSrid == 25833)
        {
            var transformedCoordinate = CoordinateSystemWkt.Transformation_4326_to_25833.MathTransform.Transform(new double[] { lon, lat });

            return transformedCoordinate;
        }

        return null;
    }

    private static class CoordinateSystemWkt
    {
        public static readonly CoordinateSystemFactory CsFactory;
        public static readonly CoordinateTransformationFactory CtFactory;
        public static readonly CoordinateSystem Wgs84;
        public static readonly CoordinateSystem Utm33n;
        public static readonly ICoordinateTransformation Transformation_4326_to_25833;

        public static string Wgs84String = "GEOGCS[\"WGS 84\", " +
            "DATUM[\"WGS_1984\", " +
            "SPHEROID[\"WGS 84\",6378137,298.257223563,AUTHORITY[\"EPSG\",\"7030\"]], " +
            "AUTHORITY[\"EPSG\",\"6326\"]], " +
            "PRIMEM[\"Greenwich\",0,AUTHORITY[\"EPSG\",\"8901\"]], " +
            "UNIT[\"degree\",0.0174532925199433,AUTHORITY[\"EPSG\",\"9122\"]], " +
            "AUTHORITY[\"EPSG\",\"4326\"]]";

        public static string Utm33nString = "PROJCS[\"ETRS89 / UTM zone 33N\", " +
            "GEOGCS[\"ETRS89\", " +
            "DATUM[\"European_Terrestrial_Reference_System_1989\", " +
            "SPHEROID[\"GRS 1980\",6378137,298.257222101,AUTHORITY[\"EPSG\",\"7019\"]], " +
            "AUTHORITY[\"EPSG\",\"6258\"]], " +
            "PRIMEM[\"Greenwich\",0,AUTHORITY[\"EPSG\",\"8901\"]], " +
            "UNIT[\"degree\",0.0174532925199433,AUTHORITY[\"EPSG\",\"9122\"]], " +
            "AUTHORITY[\"EPSG\",\"4258\"]], " +
            "PROJECTION[\"Transverse_Mercator\"], " +
            "PARAMETER[\"latitude_of_origin\",0], " +
            "PARAMETER[\"central_meridian\",15], " +
            "PARAMETER[\"scale_factor\",0.9996], " +
            "PARAMETER[\"false_easting\",500000], " +
            "PARAMETER[\"false_northing\",0], " +
            "UNIT[\"metre\",1,AUTHORITY[\"EPSG\",\"9001\"]], " +
            "AUTHORITY[\"EPSG\",\"25833\"]]";

        static CoordinateSystemWkt()
        {
            CsFactory = new CoordinateSystemFactory();
            CtFactory = new CoordinateTransformationFactory();

            Wgs84 = (CoordinateSystem)CsFactory.CreateFromWkt(Wgs84String);
            Utm33n = (CoordinateSystem)CsFactory.CreateFromWkt(Utm33nString);

            Transformation_4326_to_25833 = CtFactory.CreateFromCoordinateSystems(Wgs84, Utm33n);
        }
    }



    /*
    public async Task<int> TransformGeometry(NetTopologySuite.Geometries.Geometry geometry, int srid)
    {
        int sourceSrid = 4326; // WGS 84
        int targetSrid = 25833; // ETRS89 / UTM zone 33N

        // Create the coordinate systems from SRIDs
        var sourceCsFactory = new CoordinateSystemFactory();
        var sourceCs = sourceCsFactory.CreateFromWkt(CoordinateSystemWkt.WGS84);

        var targetCsFactory = new CoordinateSystemFactory();
        var targetCs = targetCsFactory.CreateFromWkt(CoordinateSystemWkt.ETRS89_UTM33N);

        // Create the transformation
        var coordinateTransformationFactory = new CoordinateTransformationFactory();
        var transformation = coordinateTransformationFactory.CreateFromCoordinateSystems(sourceCs, targetCs);

        // Transform the geometry
        NetTopologySuite.Geometries.Geometry transformedGeometry = TransformGeometry(geometry.Factory, geometry, transformation.MathTransform);

        transformedGeometry.SRID = targetSrid;

        return 1;
    }

    private static class CoordinateSystemWkt
    {
        public static string WGS84 = "GEOGCS[\"WGS 84\",DATUM[\"WGS_1984\",SPHEROID[\"WGS 84\",6378137,298.257223563]],PRIMEM[\"Greenwich\",0],UNIT[\"degree\",0.0174532925199433]]";
        public static string ETRS89_UTM33N = "PROJCS[\"ETRS89 / UTM zone 33N\",GEOGCS[\"ETRS89\",DATUM[\"European_Terrestrial_Reference_System_1989\",SPHEROID[\"GRS 1980\",6378137,298.257222101]],PRIMEM[\"Greenwich\",0],UNIT[\"degree\",0.0174532925199433]],PROJECTION[\"Transverse_Mercator\"],PARAMETER[\"latitude_of_origin\",0],PARAMETER[\"central_meridian\",15],PARAMETER[\"scale_factor\",0.9996],PARAMETER[\"false_easting\",500000],PARAMETER[\"false_northing\",0],UNIT[\"metre\",1,AUTHORITY[\"EPSG\",\"9001\"]],AXIS[\"Easting\",EAST],AXIS[\"Northing\",NORTH],AUTHORITY[\"EPSG\",\"25833\"]]";
    }

    private NetTopologySuite.Geometries.Geometry TransformGeometry(NetTopologySuite.Geometries.GeometryFactory geometryFactory, NetTopologySuite.Geometries.Geometry geom, ProjNet.CoordinateSystems.Transformations.MathTransform transform)
    {
        if (geom is NetTopologySuite.Geometries.Point)
        {
            return geometryFactory.CreatePoint(TransformCoordinate(((NetTopologySuite.Geometries.Point)geom).Coordinate, transform));
        }
        else if (geom is NetTopologySuite.Geometries.LineString)
        {
            return geometryFactory.CreateLineString(TransformCoordinates(((NetTopologySuite.Geometries.LineString)geom).Coordinates, transform));
        }
        else if (geom is NetTopologySuite.Geometries.Polygon)
        {
            var polygon = (NetTopologySuite.Geometries.Polygon)geom;
            return TransformPolygon(geometryFactory, polygon, transform);
        }
        else if (geom is NetTopologySuite.Geometries.MultiPoint)
        {
            var points = (NetTopologySuite.Geometries.MultiPoint)geom;
            var transformedPoints = new NetTopologySuite.Geometries.Point[points.NumGeometries];
            for (int i = 0; i < points.NumGeometries; i++)
            {
                transformedPoints[i] = (NetTopologySuite.Geometries.Point)TransformGeometry(geometryFactory, points.GetGeometryN(i), transform);
            }
            return geometryFactory.CreateMultiPoint(transformedPoints);
        }
        else if (geom is NetTopologySuite.Geometries.MultiLineString)
        {
            var lineStrings = (NetTopologySuite.Geometries.MultiLineString)geom;
            return geometryFactory.CreateMultiLineString(TransformLineStrings(geometryFactory, lineStrings, transform));
        }
        else if (geom is NetTopologySuite.Geometries.MultiPolygon)
        {
            var polygons = (NetTopologySuite.Geometries.MultiPolygon)geom;
            return geometryFactory.CreateMultiPolygon(TransformPolygons(geometryFactory, polygons, transform));
        }
        else
        {
            throw new NotSupportedException("Geometry type not supported: " + geom.GeometryType);
        }
    }

    private NetTopologySuite.Geometries.Polygon TransformPolygon(NetTopologySuite.Geometries.GeometryFactory geometryFactory, NetTopologySuite.Geometries.Polygon polygon, ProjNet.CoordinateSystems.Transformations.MathTransform transform)
    {
        // Step 1: Extract the shell (outer boundary) of the polygon
        var shell = polygon.Shell;

        // Step 2: Transform the shell
        var transformedShell = (NetTopologySuite.Geometries.LinearRing)TransformGeometry(geometryFactory, shell, transform);

        // Step 3: Extract the holes (inner boundaries) of the polygon
        var holes = polygon.Holes;

        // Step 4: Transform the holes
        var transformedHoles = TransformLinearRings(geometryFactory, holes, transform);

        // Step 5: Create and return a new polygon with the transformed shell and holes
        return geometryFactory.CreatePolygon(transformedShell, transformedHoles);
    }

    private NetTopologySuite.Geometries.LinearRing TransformLinearRing(NetTopologySuite.Geometries.GeometryFactory geometryFactory, NetTopologySuite.Geometries.LinearRing ring, ProjNet.CoordinateSystems.Transformations.MathTransform transform)
    {
        return (NetTopologySuite.Geometries.LinearRing)TransformGeometry(geometryFactory, ring, transform);
    }

    private NetTopologySuite.Geometries.LinearRing[] TransformLinearRings(NetTopologySuite.Geometries.GeometryFactory geometryFactory, NetTopologySuite.Geometries.LinearRing[] rings, ProjNet.CoordinateSystems.Transformations.MathTransform transform)
    {
        var transformedRings = new NetTopologySuite.Geometries.LinearRing[rings.Length];
        for (int i = 0; i < rings.Length; i++)
        {
            transformedRings[i] = TransformLinearRing(geometryFactory, rings[i], transform);
        }
        return transformedRings;
    }

    private NetTopologySuite.Geometries.Coordinate TransformCoordinate(NetTopologySuite.Geometries.Coordinate coord, ProjNet.CoordinateSystems.Transformations.MathTransform transform)
    {
        var transformedCoord = transform.Transform(new[] { coord.X, coord.Y });
        return new NetTopologySuite.Geometries.Coordinate(transformedCoord[0], transformedCoord[1]);
    }

    private NetTopologySuite.Geometries.Coordinate[] TransformCoordinates(NetTopologySuite.Geometries.Coordinate[] coords, ProjNet.CoordinateSystems.Transformations.MathTransform transform)
    {
        var transformedCoords = new NetTopologySuite.Geometries.Coordinate[coords.Length];
        for (int i = 0; i < coords.Length; i++)
        {
            transformedCoords[i] = TransformCoordinate(coords[i], transform);
        }
        return transformedCoords;
    }

    private NetTopologySuite.Geometries.LineString[] TransformLineStrings(NetTopologySuite.Geometries.GeometryFactory geometryFactory, NetTopologySuite.Geometries.MultiLineString lineStrings, ProjNet.CoordinateSystems.Transformations.MathTransform transform)
    {
        var transformedLineStrings = new NetTopologySuite.Geometries.LineString[lineStrings.NumGeometries];
        for (int i = 0; i < lineStrings.NumGeometries; i++)
        {
            transformedLineStrings[i] = (NetTopologySuite.Geometries.LineString)TransformGeometry(geometryFactory, lineStrings.GetGeometryN(i), transform);
        }
        return transformedLineStrings;
    }

    private NetTopologySuite.Geometries.Polygon[] TransformPolygons(NetTopologySuite.Geometries.GeometryFactory geometryFactory, NetTopologySuite.Geometries.MultiPolygon polygons, ProjNet.CoordinateSystems.Transformations.MathTransform transform)
    {
        var transformedPolygons = new NetTopologySuite.Geometries.Polygon[polygons.NumGeometries];
        for (int i = 0; i < polygons.NumGeometries; i++)
        {
            transformedPolygons[i] = (NetTopologySuite.Geometries.Polygon)TransformGeometry(geometryFactory, polygons.GetGeometryN(i), transform);
        }
        return transformedPolygons;
    }
    */
}
