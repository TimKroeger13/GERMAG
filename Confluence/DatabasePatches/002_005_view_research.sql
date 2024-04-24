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

--Main
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


--Table creation
--- ax_tree
CREATE TABLE ax_tree AS
SELECT tree.geom AS geom
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
    WHERE parameter_key = 36 OR parameter_key = 37
) AS tree
WHERE ST_Covers(usable_area.geom, tree.geom)



--ax_buildings
CREATE TABLE ax_buildings AS
SELECT building.geom AS geom
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
    WHERE parameter_key = 32
) AS building
WHERE ST_Covers(usable_area.geom, building.geom)


--BU Tree Area
CREATE TABLE bu_ax_tree AS
SELECT st_buffer(st_union(geom),4) FROM ax_tree


--BU Selected Area
CREATE TABLE bu_ax_selected AS
SELECT st_buffer(st_union(ST_ExteriorRing(geom)),3) FROM ax_selected












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