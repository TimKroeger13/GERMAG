using AutoMapper.Features;
using GERMAG.DataModel.Database;
using System.Linq;
using System.Text.Json;

namespace GERMAG.Server.DataPulling.JsonDeserialize;

public interface IJsonDeserialize
{
    Root DeserializeJson(string SerializedInputJson, TypeOfData typeOfData, JsonFormat format);
}
//#pragma warning disable IDE1006 // Naming Styles
public class Crs
{
    public string? Type { get; set; }
    public Properties? Properties { get; set; }
}
public class Feature
{
    public string? Type { get; set; }
    public string? Id { get; set; }
    public Geometry? Geometry { get; set; }
    public Properties? Properties { get; set; }
    public string? Geometry_name { get; set; }
    public List<double>? Bbox { get; set; }
}
public class Geometry
{
    public string? Type { get; set; }
    public List<double>? CoordinatesSingle { get; set; }
    public List<List<double>>? CoordinatesShort { get; set; }
    public List<List<List<double>>>? Coordinates { get; set; }
    public List<List<List<List<double>>>>? CoordinatesLong { get; set; }
}
public class Properties
{
    public string? Gridcode { get; set; }
    public string? La_40txt { get; set; }
    public string? La_60txt { get; set; }
    public string? La_80txt { get; set; }
    public string? La_100txt { get; set; }
    public string? La_100xt { get; set; }
    public string? Name { get; set; }
    public string? La_100 { get; set; }
    public string? Text { get; set; }
    public string? Bezeich { get; set; }
    public string? Afl { get; set; }
    public string? Fsko { get; set; }
    public string? Zae { get; set; }
    public string? Nen { get; set; }
    public string? Gmk { get; set; }
    public string? Namgmk { get; set; }
    public string? Fln { get; set; }
    public string? Gdz { get; set; }
    public string? Namgem { get; set; }
    public string? Zde { get; set; }
    public string? Dst { get; set; }
    public DateTime? Beg { get; set; }
    public DateTime? Statusdat { get; set; }
    public string? Uuid { get; set; }
    public int? Importid { get; set; }
    public string? Hoehe { get; set; }
    public string? Herkunft { get; set; }
    public string? Name_karte { get; set; }
    public string? Nh4 { get; set; }
    public string? Lf { get; set; }
    public string? Cl { get; set; }
    public string? Po4 { get; set; }
    public string? So4 { get; set; }
    public string? K { get; set; }
    public string? B { get; set; }
    public string? Csv { get; set; }
    public string? Point_x { get; set; }
    public string? Point_y { get; set; }
    public string? Invhyas { get; set; }
    public string? Invname { get; set; }
    public string? Eigentuemer { get; set; }
    public string? Messnetz_klartext { get; set; }
    public string? Zcoordb { get; set; }
    public string? Invmbeg { get; set; }
    public string? Invzbeg { get; set; }
    public string? Invzend { get; set; }
    public string? Xcoord { get; set; }
    public string? Ycoord { get; set; }
    public string? Grwtemp_text { get; set; }
    public string? Wasserwerk { get; set; }
    public string? Gebietsnr { get; set; }
    public string? Zone { get; set; }
    public string? Verordnung { get; set; }
    public string? Datum { get; set; }
    public string? Gvbl { get; set; }
    public string? Veror_link { get; set; }
    public string? Ae_datum { get; set; }
    public string? Ae_gvbl { get; set; }
    public string? Zehgw_m_tx { get; set; }
    public string? Gfk { get; set; }
    public string? Bezgfk { get; set; }
    public string? Ofl { get; set; }
    public string? Bezofl { get; set; }
    public string? Aog { get; set; }
    public string? Aug { get; set; }
    public string? Hoh { get; set; }
    public string? Bat { get; set; }
    public string? Bezbat { get; set; }
    public string? Nam { get; set; }
    public string? Baw { get; set; }
    public string? Bezbaw { get; set; }
    public string? Zus { get; set; }
    public string? Bezzus { get; set; }
    public string? Gkn { get; set; }
    public string? Des { get; set; }
    public string? Bezdes { get; set; }
    public string? Lag { get; set; }
    public string? Namlag { get; set; }
    public string? Hnr { get; set; }
    public string? Pnr { get; set; }
    public string? Lnr { get; set; }
}
public class Root
{
    public string? Type { get; set; }
    public List<double>? Bbox { get; set; }
    public int? TotalFeatures { get; set; }
    public List<Feature>? Features { get; set; }
    public Crs? Crs { get; set; }
    public int? NumberMatched { get; set; }
    public int? NumberReturned { get; set; }
    public DateTime? TimeStamp { get; set; }
}
//#pragma warning restore IDE1006 // Naming Styles

public class JsonDeserialize() : IJsonDeserialize
{
    public Root DeserializeJson(string SerializedInputJson, TypeOfData typeOfData, JsonFormat format)
    {
        var jsonData_Root = JsonSerializer.Deserialize<Root>(SerializedInputJson, new JsonSerializerOptions()
        {
            PropertyNameCaseInsensitive = true,
        }) ?? throw new Exception("No wfs found (root)");

        if (format == JsonFormat.long_coordiantes)
        {
            if (jsonData_Root.Features?.Count > 0)
            {
                foreach (var feature in jsonData_Root.Features)
                {
                    if (feature.Geometry != null)
                    {
                        if (feature.Geometry.Coordinates == null && feature.Geometry.CoordinatesLong != null)
                        {
                            feature.Geometry.Coordinates = CopyCoordinateLongsToCoordinates(feature.Geometry.CoordinatesLong);
                            feature.Geometry.CoordinatesLong = null;
                        }
                    }
                }
            }
        }
        if (format == JsonFormat.short_coordiantes)
        {
            if (jsonData_Root.Features?.Count > 0)
            {
                foreach (var feature in jsonData_Root.Features)
                {
                    if (feature.Geometry != null)
                    {
                        if (feature.Geometry.Coordinates == null && feature.Geometry.CoordinatesShort != null)
                        {
                            feature.Geometry.Coordinates = CopyCoordinateShortToCoordinates(feature.Geometry.CoordinatesShort);
                            feature.Geometry.CoordinatesShort = null;
                        }
                    }
                }
            }
        }
        if (format == JsonFormat.single_coordiantes)
        {
            if (jsonData_Root.Features?.Count > 0)
            {
                foreach (var feature in jsonData_Root.Features)
                {
                    if (feature.Geometry != null)
                    {
                        if (feature.Geometry.Coordinates == null && feature.Geometry.CoordinatesSingle != null)
                        {
                            var doubleList = feature.Geometry.CoordinatesSingle;
                            feature.Geometry.Coordinates = [[feature.Geometry.CoordinatesSingle!]];
                            feature.Geometry.CoordinatesSingle = null;
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
    private List<List<List<double>>>? CopyCoordinateShortToCoordinates(List<List<double>>? coordinateShort)
    {
        return [coordinateShort!];
    }
}
