--Selected Area
SELECT ST_Intersection(usable_area.geom, parcel_area.geom) AS geom
FROM 
(
    SELECT ST_Union(geom) AS geom 
    FROM geo_data
    WHERE parameter_key = 
	(
		SELECT id
		FROM geothermal_parameter
		WHERE typeofdata = 'area_usage'
	)
	AND parameter ->> 'Bezeich' = 'AX_Wohnbauflaeche'
) AS usable_area,
(
    SELECT ST_Union(geom) AS geom 
    FROM geo_data
    WHERE parameter_key = 
	(
		SELECT id
		FROM geothermal_parameter
		WHERE typeofdata = 'land_parcels'
	)
) AS parcel_area


--Table creation

--- ax_selected
CREATE TABLE ax_selected AS
SELECT ST_Intersection(usable_area.geom, parcel_area.geom) AS geom
FROM 
(
    SELECT ST_Union(geom) AS geom 
    FROM geo_data
    WHERE parameter_key = 
	(
		SELECT id
		FROM geothermal_parameter
		WHERE typeofdata = 'area_usage'
	)
	AND parameter ->> 'Bezeich' = 'AX_Wohnbauflaeche'
) AS usable_area,
(
    SELECT ST_Union(geom) AS geom 
    FROM geo_data
    WHERE parameter_key = 
	(
		SELECT id
		FROM geothermal_parameter
		WHERE typeofdata = 'land_parcels'
	)
) AS parcel_area

--- ax_tree

--Trees
CREATE TABLE ax_tree AS
SELECT geom AS geom FROM geo_data
WHERE parameter_key = 36 OR parameter_key = 37


--buildings
CREATE TABLE ax_buildings AS
SELECT ST_Union(geom) AS geom FROM geo_data
WHERE parameter_key = 32


--AX_buffertrees
--CREATE TABLE ax_buffer_tree AS
SELECT ST_Buffer(geom,4) FROM ax_tree
--12 sec

SELECT ST_Buffer(geom,6) FROM ax_buildings
--20