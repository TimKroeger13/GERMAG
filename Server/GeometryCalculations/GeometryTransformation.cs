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
using GERMAG.DataModel.Database;
using GERMAG.Shared;

namespace GERMAG.Server.GeometryCalculations;

public interface IGeometryTransformation
{
    Task<LandParcel> TransformGeometry(NetTopologySuite.Geometries.Geometry geometry, int srid);
}

public class GeometryTransformation(DataContext context) : IGeometryTransformation
{
    public async Task<LandParcel> TransformGeometry(NetTopologySuite.Geometries.Geometry geometry, int SourceSrid)
    {
        var TagedSrid = 25833;
        var geoJsonWriter = new GeoJsonWriter();

        if (geometry is NetTopologySuite.Geometries.Polygon)
        {

            for (int i = 0; i < geometry.Coordinates.Count(); i++)
            {
                var RawCoordinates = geometry.Coordinates[i];

                var transformedCoordinate = await transformCoordinates(RawCoordinates[0], RawCoordinates[1], SourceSrid, TagedSrid) ?? throw new Exception("Coordiantes could not be transformed");
                geometry.Coordinates[i][0] = transformedCoordinate[0];
                geometry.Coordinates[i][1] = transformedCoordinate[1];
            }
        }

        geometry.SRID = TagedSrid;

        var landParcelID = context.GeothermalParameter.First(gp => gp.Type == TypeOfData.land_parcels).Id;

        var returnResult = new LandParcel
        {
            GeoDataID = landParcelID,
            ParameterKey = landParcelID,
            Parameter = "",
            Geometry = geometry,
            GeometryJson = geoJsonWriter.Write(geometry),
        };

        return returnResult;
    }



    async private Task<double[]?> transformCoordinates (double lon, double lat, int SourceSrid, int TagedSrid)
    {
        return await Task.Run(() =>
            {
                if (SourceSrid == 4326 && TagedSrid == 25833)
                {
                    var transformedCoordinate = CoordinateSystemWkt.Transformation_4326_to_25833.MathTransform.Transform(new double[] { lon, lat });

                    return transformedCoordinate;
                }

            return null;
        });
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
}
