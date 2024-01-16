using GERMAG.DataModel.Database;
using System.Text.Json;

namespace GERMAG.Server.DataPulling.JsonDeserialize;

public interface IJsonDeserialize
{
    Root ChooseDeserializationJson(string SeriallizedInputJson, TypeOfData typeOfData);
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

    //land parcel
}
public class Properties
{
    public string? gridcode { get; set; }
    public string? la_100txt { get; set; }
    public string? name { get; set; }
    public string? la_100 { get; set; }
    public string? text { get; set; }
}
public class Root
{
    public string? type { get; set; }
    public List<double>? bbox { get; set; }
    public int totalFeatures { get; set; }
    public List<Feature>? features { get; set; }
    public Crs? crs { get; set; }
}

//LongCoordinateFormat
public class CrsLong
{
    public string? type { get; set; }
    public PropertiesLong? properties { get; set; }
}
public class FeatureLong
{
    public string? type { get; set; }
    public string? id { get; set; }
    public GeometryLong? geometry { get; set; }
    public PropertiesLong? properties { get; set; }
}
public class GeometryLong
{
    public string? type { get; set; }
    public List<List<List<List<double>>>>? coordinates { get; set; }

    //land parcel
}
public class PropertiesLong
{
    public string? gridcode { get; set; }
    public string? la_100txt { get; set; }
    public string? name { get; set; }
    public string? la_100 { get; set; }
    public string? text { get; set; }
}
public class RootLong
{
    public string? type { get; set; }
    public List<double>? bbox { get; set; }
    public int totalFeatures { get; set; }
    public List<FeatureLong>? features { get; set; }
    public CrsLong? crs { get; set; }
}

#pragma warning restore IDE1006 // Naming Styles

public class JsonDeserialize() : IJsonDeserialize
{
    public Root ChooseDeserializationJson(string SeriallizedInputJson, TypeOfData typeOfData)
    {
        if (typeOfData == TypeOfData.geo_poten_restrict)
        {
            RootLong? x = JsonSerializer.Deserialize<RootLong>(SeriallizedInputJson) ?? throw new Exception("No wfs found (root)");
        }

        Root? jsonData_Root = JsonSerializer.Deserialize<Root>(SeriallizedInputJson) ?? throw new Exception("No wfs found (root)");
        return jsonData_Root;
    }
}
