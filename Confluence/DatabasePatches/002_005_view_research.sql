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


--Killer statement All intersections
CREATE TABLE ax_selected AS
SELECT ST_UNION(ST_Intersection(usable_area.geom, parcel_area.geom)) AS geom
FROM 
(
    SELECT ST_UNION(geom) AS geom 
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
    SELECT geom AS geom 
    FROM geo_data
    WHERE parameter_key = 
	(
		SELECT id
		FROM geothermal_parameter
		WHERE typeofdata = 'land_parcels'
	)
) AS parcel_area


-- possible network distance
SELECT ST_Length(
    ST_LineSubstring(
        line.geom,
        ST_LineLocatePoint(line.geom, source_point.geom),
        ST_LineLocatePoint(line.geom, destination_point.geom)
    )
) AS network_distance
FROM lines AS line, points AS source_point, points AS destination_point
WHERE source_point.id = <source_point_id>
AND destination_point.id = <destination_point_id>;