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

-- Usage area
CREATE TABLE usage_area AS
SELECT parameter ->> 'Uuid' AS uuid,parameter ->> 'Bezeich' AS bezeich,
geom AS geom 
FROM geo_data
WHERE parameter_key = 
(
	SELECT id
	FROM geothermal_parameter
	WHERE typeofdata = 'area_usage'
)


--- ax_tree

--Trees
CREATE TABLE ax_tree AS
SELECT geom AS geom FROM geo_data
WHERE parameter_key = 36 OR parameter_key = 37


--ax_buildings
CREATE TABLE ax_buildings AS
SELECT geom AS geom FROM geo_data
WHERE parameter_key = 32

--ax_buildings_test
CREATE TABLE ax_buildings AS
SELECT geom AS geom FROM geo_data
WHERE parameter_key = 32

--ax_select_test
CREATE TABLE ax_selected_test AS
SELECT (ST_Dump(ST_UNION(parcel_area.geom))).geom AS geom
FROM 
(
	SELECT geom AS geom 
	FROM geo_data
	WHERE parameter_key = 
		(
			SELECT id
			FROM geothermal_parameter
			WHERE typeofdata = 'area_usage'
		)
	AND parameter ->> 'Bezeich' = 'AX_Wohnbauflaeche'
	AND (
	parameter ->> 'Uuid' = 'DEBE02YY40001dGH'
	OR
	parameter ->> 'Uuid' = 'DEBE02YY40001cyw'
	)
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
WHERE ST_Covers(usable_area.geom, parcel_area.geom)
AND ST_GeometryType(parcel_area.geom) = 'ST_Polygon'


--ax_select (Killer statement All intersections) 
CREATE TABLE ax_selected AS
SELECT (ST_Dump(ST_UNION(parcel_area.geom))).geom AS geom
FROM 
(
	SELECT geom AS geom 
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
WHERE ST_Covers(usable_area.geom, parcel_area.geom)
AND ST_GeometryType(parcel_area.geom) = 'ST_Polygon'


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