using GERMAG.Server.ExtensionMethods;
using GERMAG.Shared;
using NetTopologySuite.Geometries;
using System.Text;

namespace GERMAG.Server.ReportCreation;

public interface ICreateReportStructure
{
    Report[] CreateReport(List<CoordinateParameters> CoordinateParameters, double Xcor, double Ycor, int Srid);
}

public class CreateReportStructure : ICreateReportStructure
{
    private List<String>? _geo_poten_100m_with_2400ha = [];
    private List<String>? _geo_poten_80m_with_2400ha = [];
    private List<String>? _geo_poten_60m_with_2400ha = [];
    private List<String>? _geo_poten_40m_with_2400ha = [];
    private List<String>? _geo_poten_100m_with_1800ha = [];
    private List<String>? _geo_poten_80m_with_1800ha = [];
    private List<String>? _geo_poten_60m_with_1800ha = [];
    private List<String>? _geo_poten_40m_with_1800ha = [];
    private List<String>? _thermal_con_100 = [];
    private List<String>? _thermal_con_80 = [];
    private List<String>? _thermal_con_60 = [];
    private List<String>? _thermal_con_40 = [];
    private List<String>? _mean_water_temp_20to100 = [];
    private List<String>? _mean_water_temp_60 = [];
    private List<String>? _mean_water_temp_40 = [];
    private List<String>? _mean_water_temp_20 = [];
    private String? _geo_poten_restrict = "";
    private String? _water_protec_areas = "";
    private String? _land_parcel_number = "";
    private String? _land_parcels_gemeinde = "";
    private String? _geometry = null;
    private String? _building_begzgkt = "";
    private String? _zeHGW = "";
    private String? _verordnung = "";
    private String? _veror_link = "";
    public Report[] CreateReport(List<CoordinateParameters> CoordinateParameters, double Xcor, double Ycor, int Srid)
    {
        foreach (var CoordinateParameter in CoordinateParameters)
        {
            switch (CoordinateParameter.Type)
            {
                case DataModel.Database.TypeOfData.land_parcels:
                    _land_parcel_number = AppendString(_land_parcel_number ?? "", CoordinateParameter.JsonDataParameter?.Zae?.ToString() ?? "");
                    _land_parcels_gemeinde = AppendString(_land_parcels_gemeinde ?? "", CoordinateParameter.JsonDataParameter?.Namgem?.ToString() ?? "");
                    _geometry = CoordinateParameter.Geometry;
                    break;
                case DataModel.Database.TypeOfData.geo_poten_restrict:
                    _geo_poten_restrict = AppendString(_geo_poten_restrict ?? "", CoordinateParameter.JsonDataParameter?.Text?.ToString() ?? "");
                    break;
                case DataModel.Database.TypeOfData.mean_water_temp_20to100:
                    _mean_water_temp_20to100?.Add(CoordinateParameter.JsonDataParameter?.Grwtemp_text?.ToString() ?? "");
                    break;
                case DataModel.Database.TypeOfData.mean_water_temp_60:
                    _mean_water_temp_60?.Add(CoordinateParameter.JsonDataParameter?.Grwtemp_text?.ToString() ?? "");
                    break;
                case DataModel.Database.TypeOfData.mean_water_temp_40:
                    _mean_water_temp_40?.Add(CoordinateParameter.JsonDataParameter?.Grwtemp_text?.ToString() ?? "");
                    break;
                case DataModel.Database.TypeOfData.mean_water_temp_20:
                    _mean_water_temp_20?.Add(CoordinateParameter.JsonDataParameter?.Grwtemp_text?.ToString() ?? "");
                    break;
                case DataModel.Database.TypeOfData.geo_poten_100m_with_2400ha:
                    _geo_poten_100m_with_2400ha?.Add(CoordinateParameter.JsonDataParameter?.La_100txt?.ToString() ?? "");
                    break;
                case DataModel.Database.TypeOfData.geo_poten_80m_with_2400ha:
                    _geo_poten_80m_with_2400ha?.Add(CoordinateParameter.JsonDataParameter?.La_80txt?.ToString() ?? "");
                    break;
                case DataModel.Database.TypeOfData.geo_poten_60m_with_2400ha:
                    _geo_poten_60m_with_2400ha?.Add(CoordinateParameter.JsonDataParameter?.La_60txt?.ToString() ?? "");
                    break;
                case DataModel.Database.TypeOfData.geo_poten_40m_with_2400ha:
                    _geo_poten_40m_with_2400ha?.Add(CoordinateParameter.JsonDataParameter?.La_40txt?.ToString() ?? "");
                    break;
                case DataModel.Database.TypeOfData.geo_poten_100m_with_1800ha:
                    _geo_poten_100m_with_1800ha?.Add(CoordinateParameter.JsonDataParameter?.La_100xt?.ToString() ?? "");
                    break;
                case DataModel.Database.TypeOfData.geo_poten_80m_with_1800ha:
                    _geo_poten_80m_with_1800ha?.Add(CoordinateParameter.JsonDataParameter?.La_80txt?.ToString() ?? "");
                    break;
                case DataModel.Database.TypeOfData.geo_poten_60m_with_1800ha:
                    _geo_poten_60m_with_1800ha?.Add(CoordinateParameter.JsonDataParameter?.La_60txt?.ToString() ?? "");
                    break;
                case DataModel.Database.TypeOfData.geo_poten_40m_with_1800ha:
                    _geo_poten_40m_with_1800ha?.Add(CoordinateParameter.JsonDataParameter?.La_40txt?.ToString() ?? "");
                    break;
                case DataModel.Database.TypeOfData.thermal_con_40:
                    _thermal_con_40?.Add(CoordinateParameter.JsonDataParameter?.La_40txt?.ToString() ?? "");
                    break;
                case DataModel.Database.TypeOfData.thermal_con_60:
                    _thermal_con_60?.Add(CoordinateParameter.JsonDataParameter?.La_60txt?.ToString() ?? "");
                    break;
                case DataModel.Database.TypeOfData.thermal_con_80:
                    _thermal_con_80?.Add(CoordinateParameter.JsonDataParameter?.La_80txt?.ToString() ?? "");
                    break;
                case DataModel.Database.TypeOfData.thermal_con_100:
                    _thermal_con_100?.Add(CoordinateParameter.JsonDataParameter?.La_100txt?.ToString() ?? "");
                    break;
                case DataModel.Database.TypeOfData.water_protec_areas:
                    break;
                case DataModel.Database.TypeOfData.building_surfaces:
                    //_building_begzgkt = AppendString(_building_begzgkt ?? "", CoordinateParameter.JsonDataParameter?.Bezgfk?.ToString() ?? "");
                    break;

                //Cases Not relevant for the Report or right now not implemented
                case DataModel.Database.TypeOfData.groundwater_measuring_points:
                    break;
                case DataModel.Database.TypeOfData.dgm:
                    break;
                case DataModel.Database.TypeOfData.veg_height:
                    break;
                case DataModel.Database.TypeOfData.main_water_lines:
                    break;
                case DataModel.Database.TypeOfData.groundwater_surface_distance:
                    break;
                case DataModel.Database.TypeOfData.ground_water_height_main:
                    break;
                case DataModel.Database.TypeOfData.ground_water_height_tension:
                    break;
                case DataModel.Database.TypeOfData.water_ammonium:
                    break;
                case DataModel.Database.TypeOfData.water_bor:
                    break;
                case DataModel.Database.TypeOfData.water_chlor:
                    break;
                case DataModel.Database.TypeOfData.water_kalium:
                    break;
                case DataModel.Database.TypeOfData.water_sulfat:
                    break;
                case DataModel.Database.TypeOfData.water_ortho_phosphat:
                    break;
                case DataModel.Database.TypeOfData.electrical_con:
                    break;
                case DataModel.Database.TypeOfData.geodrilling_data:
                    break;
                case DataModel.Database.TypeOfData.geological_sections:
                    break;
                case DataModel.Database.TypeOfData.geo_drawing:
                    break;
                case DataModel.Database.TypeOfData.expe_max_groundwater_hight:
                    _zeHGW = AppendString(_zeHGW ?? "", CoordinateParameter.JsonDataParameter?.Zehgw_m_tx?.ToString() ?? "");
                    break;
                case null:
                    break;
            }
        }

        var report = new[] { new Report
        {
            Geo_poten_100m_with_2400ha = _geo_poten_100m_with_2400ha?.ConvertDokumentationString(),
            Geo_poten_80m_with_2400ha = _geo_poten_80m_with_2400ha?.ConvertDokumentationString(),
            Geo_poten_60m_with_2400ha = _geo_poten_60m_with_2400ha?.ConvertDokumentationString(),
            Geo_poten_40m_with_2400ha = _geo_poten_40m_with_2400ha?.ConvertDokumentationString(),
            Geo_poten_100m_with_1800ha = _geo_poten_100m_with_1800ha?.ConvertDokumentationString(),
            Geo_poten_80m_with_1800ha = _geo_poten_80m_with_1800ha?.ConvertDokumentationString(),
            Geo_poten_60m_with_1800ha = _geo_poten_60m_with_1800ha?.ConvertDokumentationString(),
            Geo_poten_40m_with_1800ha = _geo_poten_40m_with_1800ha?.ConvertDokumentationString(),
            Thermal_con_100 = _thermal_con_100?.ConvertDokumentationString(),
            Thermal_con_80 = _thermal_con_80?.ConvertDokumentationString(),
            Thermal_con_60 = _thermal_con_60?.ConvertDokumentationString(),
            Thermal_con_40 = _thermal_con_40?.ConvertDokumentationString(),
            Mean_water_temp_20to100 = _mean_water_temp_20to100?.ConvertDokumentationString(),
            Mean_water_temp_60 = _mean_water_temp_60?.ConvertDokumentationString(),
            Mean_water_temp_40 = _mean_water_temp_40?.ConvertDokumentationString(),
            Mean_water_temp_20 = _mean_water_temp_20?.ConvertDokumentationString(),
            Geo_poten_restrict = _geo_poten_restrict,
            Water_protec_areas = _water_protec_areas,
            Land_parcel_number = _land_parcel_number,
            Land_parcels_gemeinde = _land_parcels_gemeinde,
            Geometry = _geometry,
            Building_begzgkt = _building_begzgkt,
            //ZeHGW = _zeHGW <- polylines

        }};

        return report;
    }

    private String AppendString (String OriginString, String NewString)
    {
        if (string.IsNullOrEmpty(OriginString))
        {
            return NewString;
        }
        else
        {
            return $"{OriginString}, {NewString}".ToString();
        }
    }
}
