<h2>Database scaffolding:</h2>

````
Scaffold-DbContext "Host=10.140.24.238:5433;Database=postgres;Username=postgres;Password=password" Npgsql.EntityFrameworkCore.PostgreSQL -NoOnConfiguring -OutputDir ../Shared/DataModel/Database -Force -Context DataContext -Namespace GERMAG.DataModel.Database -StartupProject GERMAG.Server -Project GERMAG.Shared
````
<h2>Database creation sql:</h2>

````
CREATE EXTENSION postgis_raster;


CREATE TYPE typeofdata AS ENUM ('land_parcels','dgm','geo_poten_restrict','veg_height',
								'main_water_lines','groundwater_surface_distance','ground_water_height_main',
								'ground_water_height_tension','water_ammonium','water_bor','water_chlor',
								'water_kalium','water_sulfat','water_ortho_phosphat','electrical_con',
								'mean_water_temp_20to100','mean_water_temp_20','mean_water_temp_40',
								'mean_water_temp_60','geodrilling_data','geological_sections','geo_drawing',
								'water_protec_areas','expe_max_groundwater_hight','geo_poten_100m_with_2400ha',
								'geo_poten_100m_with_1800ha','geo_poten_80m_with_2400ha','geo_poten_80m_with_1800ha',
								'geo_poten_60m_with_2400ha','geo_poten_60m_with_1800ha','geo_poten_40m_with_2400ha',
								'geo_poten_40m_with_1800h','thermal_con_40','thermal_con_60','thermal_con_80',
								'thermal_con_100');



CREATE TYPE area AS ENUM ('berlin');

CREATE TYPE range AS ENUM ('near_range','far_range');

CREATE TYPE service AS ENUM ('restrictive','efficiency');


CREATE TABLE geothermal_parameter (
id SERIAL PRIMARY KEY NOT NULL,
typeofdata typeofdata,
area area,
range range,
getrequest TEXT,
geom geometry,
service service,
srid INT,
last_update TIMESTAMP,
last_ping TIMESTAMP
);

CREATE TABLE geo_data
(
    id SERIAL PRIMARY KEY NOT NULL,
    parameter_key integer NOT NULL,
    geom geometry,
    CONSTRAINT parameter_key FOREIGN KEY (parameter_key)
        REFERENCES public.geothermal_parameter (id)
);
````
