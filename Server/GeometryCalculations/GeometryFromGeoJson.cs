﻿using NetTopologySuite.IO;
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

public class GeometryFromGeoJson(IGeometryTransformation geometryTransformation) : IGeometryFromGeoJson
{
    public async Task<int> GetGeometryFromgeoJson(string Geojson, int srid)
    {

        var serializer = GeoJsonSerializer.Create();

        // Deserialize the GeoJSON string to a Feature
        NetTopologySuite.Features.Feature? feature;
        using (var stringReader = new System.IO.StringReader(Geojson))
        using (var jsonReader = new JsonTextReader(stringReader))
        {
            feature = serializer.Deserialize<NetTopologySuite.Features.Feature>(jsonReader);
        }

        if (feature == null)
        {
            return 0;
        }

        // Get the Geometry from the Feature
        NetTopologySuite.Geometries.Geometry? geometry = feature.Geometry;

        var x = await geometryTransformation.TransformGeometry(geometry, srid);




        return 1;
    }


}

