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


--Trees
SELECT ST_Union(geom) AS geom FROM geo_data
WHERE parameter_key = 36 OR parameter_key = 37


--buildings
SELECT ST_Union(geom) AS geom FROM geo_data
WHERE parameter_key = 32