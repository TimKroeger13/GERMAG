SELECT ST_Intersection(usable_area.geom, parcel_area.geom)
FROM (
    SELECT ST_Union(geom) AS geom 
    FROM geo_data
    WHERE parameter_key = 34 AND parameter ->> 'Bezeich' = 'AX_Wohnbauflaeche'
) AS usable_area,
(
    SELECT ST_Union(geom) AS geom 
    FROM geo_data
    WHERE parameter_key = 1
) AS parcel_area