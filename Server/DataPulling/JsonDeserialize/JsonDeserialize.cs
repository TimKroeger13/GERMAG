﻿using GERMAG.DataModel.Database;
using System.Text.Json;

namespace GERMAG.Server.DataPulling.JsonDeserialize;

public interface IJsonDeserialize
{
    Root ChooseDeserializationJson(string SerializedInputJson, TypeOfData typeOfData, JsonFormat format);
}
#pragma warning disable IDE1006 // Naming Styles
public class Crs
{
    public string? type { get; set; }
    public Properties? properties { get; set; }
}
public class Feature
{
    public string? type { get; set; }
    public string? id { get; set; }
    public Geometry? geometry { get; set; }
    public Properties? properties { get; set; }
}
public class Geometry
{
    public string? type { get; set; }
    public List<List<List<double>>>? coordinates { get; set; }
    public List<List<List<List<double>>>>? coordinateLongs { get; set; }
}
public class Properties
{
    public string? gridcode { get; set; }
    public string? la_100txt { get; set; }
    public string? name { get; set; }
    public string? la_100 { get; set; }
    public string? text { get; set; }
    public string? bezeich { get; set; }
    public string? afl { get; set; }
    public string? fsko { get; set; }
    public string? zae { get; set; }
    public string? nen { get; set; }
    public string? gmk { get; set; }
    public string? namgmk { get; set; }
    public string? fln { get; set; }
    public string? gdz { get; set; }
    public string? namgem { get; set; }
    public string? zde { get; set; }
    public string? dst { get; set; }
    public DateTime? beg { get; set; }
    public DateTime? statusdat { get; set; }
    public string? uuid { get; set; }
}
public class Root
{
    public string? type { get; set; }
    public List<double>? bbox { get; set; }
    public int totalFeatures { get; set; }
    public List<Feature>? features { get; set; }
    public Crs? crs { get; set; }
}

#pragma warning restore IDE1006 // Naming Styles

public class JsonDeserialize() : IJsonDeserialize
{
    public Root ChooseDeserializationJson(string SerializedInputJson, TypeOfData typeOfData, JsonFormat format)
    {
        Root? jsonData_Root = JsonSerializer.Deserialize<Root>(SerializedInputJson) ?? throw new Exception("No wfs found (root)");

        if (format == JsonFormat.long_coordiantes)
        {
            if (jsonData_Root.features != null && jsonData_Root.features.Count > 0)
            {
                foreach (var feature in jsonData_Root.features)
                {
                    if (feature.geometry != null)
                    {
                        if (feature.geometry.coordinates == null && feature.geometry.coordinateLongs != null)
                        {
                            feature.geometry.coordinates = CopyCoordinateLongsToCoordinates(feature.geometry.coordinateLongs);
                            feature.geometry.coordinateLongs = null;
                        }
                    }
                }
            }
        }
        return jsonData_Root;
    }
    private List<List<List<double>>>? CopyCoordinateLongsToCoordinates(List<List<List<List<double>>>>? coordinateLongs)
    {
        if (coordinateLongs == null || coordinateLongs.Count == 0)
            return null;
        return coordinateLongs[0];
    }
}
