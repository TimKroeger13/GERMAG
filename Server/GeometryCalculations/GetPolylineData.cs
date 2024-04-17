using GERMAG.DataModel.Database;
using GERMAG.Server.ReportCreation;
using GERMAG.Shared;
using System.Globalization;
using System.Linq;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;

namespace GERMAG.Server.GeometryCalculations;

public interface IGetPolylineData
{
    Task<double?> GetNearestPolylineData(LandParcel landParcelElement);
}

public class GetPolylineData(DataContext context, IParameterDeserialator parameterDeserialator) : IGetPolylineData
{
    public async Task<double?> GetNearestPolylineData(LandParcel landParcelElement)
    {

        var polyLineId = context.GeothermalParameter.Where(gp => gp.Geometry_Type == Geometry_Type.polyline)
            .Select(gp => new
            {
                gp.Type,
                gp.Id

            }).ToList();

        //ExpectedGroundWaterHight Case

        var ExpectedGroundWaterHight_info = polyLineId.FirstOrDefault(pl => pl.Type == TypeOfData.expe_max_groundwater_hight);

        if(ExpectedGroundWaterHight_info?.Id == null || landParcelElement.Geometry == null) { throw new Exception("Geometry for Line data Not Found"); }

        var ExpectedGroundWaterHight_data = context.GeoData.Where(gd => gd.ParameterKey == ExpectedGroundWaterHight_info.Id && gd.Parameter != "")
            .Select(gd => new
            {
                gd.Parameter,
                Distance = gd.Geom!.Distance(landParcelElement.Geometry.Centroid)
            }).OrderBy(gd => gd.Distance).FirstOrDefault(); //tolist

        var DeserializedParameter = await Task.Run(() => parameterDeserialator.DeserializeParameters(ExpectedGroundWaterHight_data?.Parameter ?? ""));

        if(DeserializedParameter.Zehgw_m_tx == "" || DeserializedParameter.Zehgw_m_tx == null) { return null; }

        var zehgw = Convert.ToDouble(Regex.Replace(DeserializedParameter.Zehgw_m_tx, ",", "."), CultureInfo.InvariantCulture);

        return zehgw;
    }
}
