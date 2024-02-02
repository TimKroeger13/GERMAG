﻿using NetTopologySuite.Geometries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GERMAG.Shared;

public class Report
{
    public String? Geo_poten_100m_with_2400ha { get; set; }
    public String? Geo_poten_80m_with_2400ha { get; set; }
    public String? Geo_poten_60m_with_2400ha { get; set; }
    public String? Geo_poten_40m_with_2400ha { get; set; }
    public String? Geo_poten_100m_with_1800ha { get; set; }
    public String? Geo_poten_80m_with_1800ha { get; set; }
    public String? Geo_poten_60m_with_1800ha { get; set; }
    public String? Geo_poten_40m_with_1800ha { get; set; }
    public String? Thermal_con_100 { get; set; }
    public String? Thermal_con_80 { get; set; }
    public String? Thermal_con_60 { get; set; }
    public String? Thermal_con_40 { get; set; }
    public String? Mean_water_temp_20to100 { get; set; }
    public String? Mean_water_temp_60 { get; set; }
    public String? Mean_water_temp_40 { get; set; }
    public String? Mean_water_temp_20 { get; set; }
    public String? Geo_poten_restrict { get; set; }
    public String? Water_protec_areas { get; set; }
    public String? Land_parcel_number { get; set; }
    public String? Land_parcels_gemeinde { get; set; }
    public String? Building_surfaces { get; set; }
    public String? Geometry { get; set; }
}