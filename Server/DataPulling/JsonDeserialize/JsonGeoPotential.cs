﻿using GERMAG.DataModel.Database;
using System.Text.Json;

namespace GERMAG.Server.DataPulling.JsonDeserialize;
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
}
public class Properties
{
    public string? gridcode { get; set; }
    public string? la_100txt { get; set; }
    public string? name { get; set; }
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
public interface IJsonGeoPotential
{
    Root GetGeoPotentialFromJson(string SeriallizedInputJson, TypeOfData typeOfData);
}

public class JsonGeoPotential : IJsonGeoPotential
{
    public Root GetGeoPotentialFromJson(string SeriallizedInputJson, TypeOfData typeOfData)
    {
        Root? jsonData_Root = JsonSerializer.Deserialize<Root>(SeriallizedInputJson) ?? throw new Exception("No wfs found (root)");
        return jsonData_Root;
    }
}
