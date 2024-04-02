-- Holstein data

ALTER TYPE typeofdata ADD VALUE 'depth_restrictions';

--depth restrictions Berlin (Holstien)
INSERT INTO geothermal_parameter (typeofdata, area, range, getrequest, service)
    VALUES ('depth_restrictions','berlin','near_range','holstein.geojson','restrictive');

    