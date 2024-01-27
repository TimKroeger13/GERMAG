using GERMAG.Shared;
using System.Text;

namespace GERMAG.Server.ReportCreation;

public interface ICreateReportStructure
{
    Report[] CreateReport(List<CoordinateParameters> CoordinateParameters, double Xcor, double Ycor, int Srid);
}

public class CreateReportStructure : ICreateReportStructure
{
    private String? _geo_poten_100m_with_2400ha = "";
    private String? _geo_poten_80m_with_2400ha = "";
    private String? _geo_poten_60m_with_2400ha = "";
    private String? _geo_poten_40m_with_2400ha = "";
    private String? _geo_poten_100m_with_1800ha = "";
    private String? _geo_poten_80m_with_1800ha = "";
    private String? _geo_poten_60m_with_1800ha = "";
    private String? _geo_poten_40m_with_1800ha = "";
    private String? _thermal_con_100 = "";
    private String? _thermal_con_80 = "";
    private String? _thermal_con_60 = "";
    private String? _thermal_con_40 = "";
    private String? _mean_water_temp_20to100 = "";
    private String? _mean_water_temp_60 = "";
    private String? _mean_water_temp_40 = "";
    private String? _mean_water_temp_20 = "";
    private String? _geo_poten_restrict = "";
    private String? _water_protec_areas = "";
    private String? _land_parcels_bezeichnung = "";
    private String? _land_parcels_gemeinde = "";
    public Report[] CreateReport(List<CoordinateParameters> CoordinateParameters, double Xcor, double Ycor, int Srid)
    {
        foreach (var CoordinateParameter in CoordinateParameters)
        {
            switch (CoordinateParameter.Type)
            {
                case DataModel.Database.TypeOfData.land_parcels:
                    _land_parcels_bezeichnung = AppendString(_land_parcels_bezeichnung ?? "", CoordinateParameter.JsonDataParameter?.Bezeich?.ToString() ?? "");
                    _land_parcels_gemeinde = AppendString(_land_parcels_gemeinde ?? "", CoordinateParameter.JsonDataParameter?.Namgem?.ToString() ?? "");
                    break;
                case DataModel.Database.TypeOfData.geo_poten_restrict:
                    break;
                case DataModel.Database.TypeOfData.mean_water_temp_20to100:
                    _mean_water_temp_20to100 = AppendString(_mean_water_temp_20to100 ?? "", CoordinateParameter.JsonDataParameter?.Grwtemp_text?.ToString() ?? "");
                    break;
                case DataModel.Database.TypeOfData.mean_water_temp_60:
                    _mean_water_temp_60 = AppendString(_mean_water_temp_60 ?? "", CoordinateParameter.JsonDataParameter?.Grwtemp_text?.ToString() ?? "");
                    break;
                case DataModel.Database.TypeOfData.mean_water_temp_40:
                    _mean_water_temp_40 = AppendString(_mean_water_temp_40 ?? "", CoordinateParameter.JsonDataParameter?.Grwtemp_text?.ToString() ?? "");
                    break;
                case DataModel.Database.TypeOfData.mean_water_temp_20:
                    _mean_water_temp_20 = AppendString(_mean_water_temp_20 ?? "", CoordinateParameter.JsonDataParameter?.Grwtemp_text?.ToString() ?? "");
                    break;
                case DataModel.Database.TypeOfData.geo_poten_100m_with_2400ha:
                    _geo_poten_100m_with_2400ha = AppendString(_geo_poten_100m_with_2400ha ?? "", CoordinateParameter.JsonDataParameter?.La_100txt?.ToString() ?? "");
                    break;
                case DataModel.Database.TypeOfData.geo_poten_80m_with_2400ha:
                    _geo_poten_80m_with_2400ha = AppendString(_geo_poten_80m_with_2400ha ?? "", CoordinateParameter.JsonDataParameter?.La_80txt?.ToString() ?? "");
                    break;
                case DataModel.Database.TypeOfData.geo_poten_60m_with_2400ha:
                    _geo_poten_60m_with_2400ha = AppendString(_geo_poten_60m_with_2400ha ?? "", CoordinateParameter.JsonDataParameter?.La_60txt?.ToString() ?? "");
                    break;
                case DataModel.Database.TypeOfData.geo_poten_40m_with_2400ha:
                    _geo_poten_40m_with_2400ha = AppendString(_geo_poten_40m_with_2400ha ?? "", CoordinateParameter.JsonDataParameter?.La_40txt?.ToString() ?? "");
                    break;
                case DataModel.Database.TypeOfData.geo_poten_100m_with_1800ha:
                    _geo_poten_100m_with_1800ha = AppendString(_geo_poten_100m_with_1800ha ?? "", CoordinateParameter.JsonDataParameter?.La_100xt?.ToString() ?? "");
                    break;
                case DataModel.Database.TypeOfData.geo_poten_80m_with_1800ha:
                    _geo_poten_80m_with_1800ha = AppendString(_geo_poten_80m_with_1800ha ?? "", CoordinateParameter.JsonDataParameter?.La_80txt?.ToString() ?? "");
                    break;
                case DataModel.Database.TypeOfData.geo_poten_60m_with_1800ha:
                    _geo_poten_60m_with_1800ha = AppendString(_geo_poten_60m_with_1800ha ?? "", CoordinateParameter.JsonDataParameter?.La_60txt?.ToString() ?? "");
                    break;
                case DataModel.Database.TypeOfData.geo_poten_40m_with_1800ha:
                    _geo_poten_40m_with_1800ha = AppendString(_geo_poten_40m_with_1800ha ?? "", CoordinateParameter.JsonDataParameter?.La_40txt?.ToString() ?? "");
                    break;
                case DataModel.Database.TypeOfData.thermal_con_40:
                    _thermal_con_40 = AppendString(_thermal_con_40 ?? "", CoordinateParameter.JsonDataParameter?.La_40txt?.ToString() ?? "");
                    break;
                case DataModel.Database.TypeOfData.thermal_con_60:
                    _thermal_con_60 = AppendString(_thermal_con_60 ?? "", CoordinateParameter.JsonDataParameter?.La_60txt?.ToString() ?? "");
                    break;
                case DataModel.Database.TypeOfData.thermal_con_80:
                    _thermal_con_80 = AppendString(_thermal_con_80 ?? "", CoordinateParameter.JsonDataParameter?.La_80txt?.ToString() ?? "");
                    break;
                case DataModel.Database.TypeOfData.thermal_con_100:
                    _thermal_con_100 = AppendString(_thermal_con_100 ?? "", CoordinateParameter.JsonDataParameter?.La_100txt?.ToString() ?? "");
                    break;
                case DataModel.Database.TypeOfData.water_protec_areas:
                    break;
                    //Cases Not relevant for the Report or right now not implemented
                case DataModel.Database.TypeOfData.groundwater_measuring_points:
                case DataModel.Database.TypeOfData.dgm:
                case DataModel.Database.TypeOfData.veg_height:
                case DataModel.Database.TypeOfData.main_water_lines:
                case DataModel.Database.TypeOfData.groundwater_surface_distance:
                case DataModel.Database.TypeOfData.ground_water_height_main:
                case DataModel.Database.TypeOfData.ground_water_height_tension:
                case DataModel.Database.TypeOfData.water_ammonium:
                case DataModel.Database.TypeOfData.water_bor:
                case DataModel.Database.TypeOfData.water_chlor:
                case DataModel.Database.TypeOfData.water_kalium:
                case DataModel.Database.TypeOfData.water_sulfat:
                case DataModel.Database.TypeOfData.water_ortho_phosphat:
                case DataModel.Database.TypeOfData.electrical_con:
                case DataModel.Database.TypeOfData.geodrilling_data:
                case DataModel.Database.TypeOfData.geological_sections:
                case DataModel.Database.TypeOfData.geo_drawing:
                case DataModel.Database.TypeOfData.expe_max_groundwater_hight:
                    break;
                case null:
                    break;
            }
        }

        var report = new[] { new Report
        {
            Geo_poten_100m_with_2400ha = _geo_poten_100m_with_2400ha,
            Geo_poten_80m_with_2400ha = _geo_poten_80m_with_2400ha,
            Geo_poten_60m_with_2400ha = _geo_poten_60m_with_2400ha,
            Geo_poten_40m_with_2400ha = _geo_poten_40m_with_2400ha,
            Geo_poten_100m_with_1800ha = _geo_poten_100m_with_1800ha,
            Geo_poten_80m_with_1800ha = _geo_poten_80m_with_1800ha,
            Geo_poten_60m_with_1800ha = _geo_poten_60m_with_1800ha,
            Geo_poten_40m_with_1800ha = _geo_poten_40m_with_1800ha,
            Thermal_con_100 = _thermal_con_100,
            Thermal_con_80 = _thermal_con_80,
            Thermal_con_60 = _thermal_con_60,
            Thermal_con_40 = _thermal_con_40,
            Mean_water_temp_20to100 = _mean_water_temp_20to100,
            Mean_water_temp_60 = _mean_water_temp_60,
            Mean_water_temp_40 = _mean_water_temp_40,
            Mean_water_temp_20 = _mean_water_temp_20,
            Geo_poten_restrict = _geo_poten_restrict,
            Water_protec_areas = _water_protec_areas,
            Land_parcels_bezeichnung = _land_parcels_bezeichnung,
            Land_parcels_gemeinde = _land_parcels_gemeinde
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
