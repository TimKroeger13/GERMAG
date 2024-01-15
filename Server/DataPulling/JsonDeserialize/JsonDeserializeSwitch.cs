using GERMAG.DataModel.Database;

namespace GERMAG.Server.DataPulling.JsonDeserialize;

public interface IJsonDeserializeSwitch
{
    Root ChooseDeserializationJson(string SeriallizedInputJson, TypeOfData typeOfData);
}

public class JsonDeserializeSwitch(IJsonGeoPotential jsonGeoPotential) : IJsonDeserializeSwitch
{
    public Root ChooseDeserializationJson(string SeriallizedInputJson, TypeOfData typeOfData)
    {
        switch (typeOfData)
        {
            case TypeOfData.land_parcels:
                break;
            case TypeOfData.dgm:
                break;
            case TypeOfData.geo_poten_restrict:
                break;
            case TypeOfData.veg_height:
                break;
            case TypeOfData.main_water_lines:
                break;
            case TypeOfData.groundwater_surface_distance:
                break;
            case TypeOfData.ground_water_height_main:
                break;
            case TypeOfData.ground_water_height_tension:
                break;
            case TypeOfData.water_ammonium:
                break;
            case TypeOfData.water_bor:
                break;
            case TypeOfData.water_chlor:
                break;
            case TypeOfData.water_kalium:
                break;
            case TypeOfData.water_sulfat:
                break;
            case TypeOfData.water_ortho_phosphat:
                break;
            case TypeOfData.electrical_con:
                break;
            case TypeOfData.mean_water_temp_20to100:
                break;
            case TypeOfData.mean_water_temp_20:
            case TypeOfData.mean_water_temp_40:
            case TypeOfData.mean_water_temp_60:
                break;
            case TypeOfData.geodrilling_data:
                break;
            case TypeOfData.geological_sections:
                break;
            case TypeOfData.geo_drawing:
                break;
            case TypeOfData.water_protec_areas:
                break;
            case TypeOfData.expe_max_groundwater_hight:
                break;
            case TypeOfData.geo_poten_100m_with_2400ha:
            case TypeOfData.geo_poten_100m_with_1800ha:
            case TypeOfData.geo_poten_80m_with_2400ha:
            case TypeOfData.geo_poten_80m_with_1800ha:
            case TypeOfData.geo_poten_60m_with_2400ha:
            case TypeOfData.geo_poten_60m_with_1800ha:
            case TypeOfData.geo_poten_40m_with_2400ha:
            case TypeOfData.geo_poten_40m_with_1800h:
            return jsonGeoPotential.GetGeoPotentialFromJson(SeriallizedInputJson, typeOfData);
            case TypeOfData.thermal_con_40:
            case TypeOfData.thermal_con_60:
            case TypeOfData.thermal_con_80:
            case TypeOfData.thermal_con_100:
                break;
            default:
                break;
        }

        throw new Exception("No fitting Deserialization Json found");
    }
}
