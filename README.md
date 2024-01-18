<h2>Database scaffolding:</h2>

````
Scaffold-DbContext "Host=192.168.1.25:5433;Database=postgres;Username=postgres;Password=password" Npgsql.EntityFrameworkCore.PostgreSQL -NoOnConfiguring -OutputDir ../Shared/DataModel/Database -Force -Context DataContext -Namespace GERMAG.DataModel.Database -StartupProject GERMAG.Server -Project GERMAG.Shared
````
<h2>Database creation sql:</h2>

````
-- CREATE EXTENSION postgis_raster;


CREATE TYPE typeofdata AS ENUM ('land_parcels','dgm','geo_poten_restrict','veg_height',
								'main_water_lines','groundwater_surface_distance','ground_water_height_main',
								'ground_water_height_tension','water_ammonium','water_bor','water_chlor',
								'water_kalium','water_sulfat','water_ortho_phosphat','electrical_con',
								'mean_water_temp_20to100','mean_water_temp_20','mean_water_temp_40',
								'mean_water_temp_60','geodrilling_data','geological_sections','geo_drawing',
								'water_protec_areas','expe_max_groundwater_hight','geo_poten_100m_with_2400ha',
								'geo_poten_100m_with_1800ha','geo_poten_80m_with_2400ha','geo_poten_80m_with_1800ha',
								'geo_poten_60m_with_2400ha','geo_poten_60m_with_1800ha','geo_poten_40m_with_2400ha',
								'geo_poten_40m_with_1800ha','thermal_con_40','thermal_con_60','thermal_con_80',
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
service service,
srid INT,
last_update TIMESTAMP,
last_ping TIMESTAMP,
hash BIGINT
);

CREATE TABLE geo_data
(
    id SERIAL PRIMARY KEY NOT NULL,
    parameter_key integer NOT NULL,
    geom geometry,
    parameter json,
    CONSTRAINT parameter_key FOREIGN KEY (parameter_key)
        REFERENCES public.geothermal_parameter (id)
);


--Flurstücke
INSERT INTO geothermal_parameter (typeofdata, area, range, getrequest, service)
    VALUES ('land_parcels','berlin','near_range','https://fbinter.stadt-berlin.de/fb/wfs/data/senstadt/s_wfs_alkis?service=wfs&version=2.0.0&request=GetFeature&typeNames=fis:s_wfs_alkis&outputFormat=application/json','restrictive');


--Entzugsleistung 40 m, für 1800 h/a
INSERT INTO geothermal_parameter (typeofdata, area, range, getrequest, service)
    VALUES ('geo_poten_40m_with_1800ha','berlin','near_range','https://fbinter.stadt-berlin.de/fb/wfs/data/senstadt/s_poly_entzugspot1800_40?service=wfs&version=2.0.0&request=GetFeature&typeNames=fis:s_poly_entzugspot1800_40&outputFormat=application/json','efficiency');

--Entzugsleistung 40 m, für 2400 h/a
INSERT INTO geothermal_parameter (typeofdata, area, range, getrequest, service)
    VALUES ('geo_poten_40m_with_2400ha','berlin','near_range','https://fbinter.stadt-berlin.de/fb/wfs/data/senstadt/s_poly_entzugspot2400_40?service=wfs&version=2.0.0&request=GetFeature&typeNames=fis:s_poly_entzugspot2400_40&outputFormat=application/json','efficiency');

--Entzugsleistung 60 m, für 1800 h/a
INSERT INTO geothermal_parameter (typeofdata, area, range, getrequest, service)
    VALUES ('geo_poten_60m_with_1800ha','berlin','near_range','https://fbinter.stadt-berlin.de/fb/wfs/data/senstadt/s_poly_entzugspot1800_60?service=wfs&version=2.0.0&request=GetFeature&typeNames=fis:s_poly_entzugspot1800_60&outputFormat=application/json','efficiency');

--Entzugsleistung 60 m, für 2400 h/a
INSERT INTO geothermal_parameter (typeofdata, area, range, getrequest, service)
    VALUES ('geo_poten_60m_with_2400ha','berlin','near_range','https://fbinter.stadt-berlin.de/fb/wfs/data/senstadt/s_poly_entzugspot2400_60?service=wfs&version=2.0.0&request=GetFeature&typeNames=fis:s_poly_entzugspot2400_60&outputFormat=application/json','efficiency');

--Entzugsleistung 80 m, für 1800 h/a
INSERT INTO geothermal_parameter (typeofdata, area, range, getrequest, service)
    VALUES ('geo_poten_80m_with_1800ha','berlin','near_range','https://fbinter.stadt-berlin.de/fb/wfs/data/senstadt/s_poly_entzugspot1800_80?service=wfs&version=2.0.0&request=GetFeature&typeNames=fis:s_poly_entzugspot1800_80&outputFormat=application/json','efficiency');

--Entzugsleistung 80 m, für 2400 h/a
INSERT INTO geothermal_parameter (typeofdata, area, range, getrequest, service)
    VALUES ('geo_poten_80m_with_2400ha','berlin','near_range','https://fbinter.stadt-berlin.de/fb/wfs/data/senstadt/s_poly_entzugspot2400_80?service=wfs&version=2.0.0&request=GetFeature&typeNames=fis:s_poly_entzugspot2400_80&outputFormat=application/json','efficiency');

--Entzugsleistung 100 m, für 2400 h/a
INSERT INTO geothermal_parameter (typeofdata, area, range, getrequest, service)
    VALUES ('geo_poten_100m_with_2400ha','berlin','near_range','https://fbinter.stadt-berlin.de/fb/wfs/data/senstadt/s_poly_entzugspot2400_100?service=wfs&version=2.0.0&request=GetFeature&typeNames=fis:s_poly_entzugspot2400_100&outputFormat=application/json','efficiency');

--Entzugsleistung 100 m, für 1800 h/a
INSERT INTO geothermal_parameter (typeofdata, area, range, getrequest, service)
    VALUES ('geo_poten_100m_with_1800ha','berlin','near_range','https://fbinter.stadt-berlin.de/fb/wfs/data/senstadt/s_poly_entzugspot1800_100?service=wfs&version=2.0.0&request=GetFeature&typeNames=fis:s_poly_entzugspot1800_100&outputFormat=application/json','efficiency');


--Geothermisches Potenzial - spezifische Wärmeleitfähigkeit bis 40 m
INSERT INTO geothermal_parameter (typeofdata, area, range, getrequest, service)
    VALUES ('thermal_con_40','berlin','near_range','https://fbinter.stadt-berlin.de/fb/wfs/data/senstadt/s_poly_wleit_40?service=wfs&version=2.0.0&request=GetFeature&typeNames=fis:s_poly_wleit_40&outputFormat=application/json','efficiency');

--Geothermisches Potenzial - spezifische Wärmeleitfähigkeit bis 60 m
INSERT INTO geothermal_parameter (typeofdata, area, range, getrequest, service)
    VALUES ('thermal_con_60','berlin','near_range','https://fbinter.stadt-berlin.de/fb/wfs/data/senstadt/s_poly_wleit_60?service=wfs&version=2.0.0&request=GetFeature&typeNames=fis:s_poly_wleit_60&outputFormat=application/json','efficiency');

--Geothermisches Potenzial - spezifische Wärmeleitfähigkeit bis 80 m
INSERT INTO geothermal_parameter (typeofdata, area, range, getrequest, service)
    VALUES ('thermal_con_80','berlin','near_range','https://fbinter.stadt-berlin.de/fb/wfs/data/senstadt/s_poly_wleit_80?service=wfs&version=2.0.0&request=GetFeature&typeNames=fis:s_poly_wleit_80&outputFormat=application/json','efficiency');

--Geothermisches Potenzial - spezifische Wärmeleitfähigkeit bis 100 m
INSERT INTO geothermal_parameter (typeofdata, area, range, getrequest, service)
    VALUES ('thermal_con_100','berlin','near_range','https://fbinter.stadt-berlin.de/fb/wfs/data/senstadt/s_poly_wleit_100?service=wfs&version=2.0.0&request=GetFeature&typeNames=fis:s_poly_wleit_100&outputFormat=application/json','efficiency');


--Geothermisches Potenzial - Restriktionsflächen
INSERT INTO geothermal_parameter (typeofdata, area, range, getrequest, service)
    VALUES ('geo_poten_restrict','berlin','near_range','https://fbinter.stadt-berlin.de/fb/wfs/data/senstadt/s_02_18_restrict_2017?service=wfs&version=2.0.0&request=GetFeature&typeNames=fis:s_02_18_restrict_2017&outputFormat=application/json','restrictive');


--Flächen Hauptgrundwasserleiter gespannt
INSERT INTO geothermal_parameter (typeofdata, area, range, getrequest, service)
    VALUES ('groundwater_surface_distance','berlin','near_range','https://gdi.berlin.de/services/wfs/ua_flurabstand_2020?service=wfs&version=2.0.0&request=GetFeature&typeNames=ua_flurabstand_2020:a_panketal&outputFormat=application/json','efficiency');

````
````
DROP TABLE geo_data;
DROP TABLE geothermal_parameter;
DROP TYPE typeofdata;
DROP TYPE area;
DROP TYPE range;
DROP TYPE service;
````
