<h2>Database scaffolding:</h2>

````
Scaffold-DbContext "Host=192.168.1.25:5433;Database=postgres;Username=postgres;Password=password" Npgsql.EntityFrameworkCore.PostgreSQL -NoOnConfiguring -OutputDir ../Shared/DataModel/Database -Force -Context DataContext -Namespace GERMAG.DataModel.Database -StartupProject GERMAG.Server -Project GERMAG.Shared
````
Referencen:

| Name         | Enum | IsInDatabase |
|--------------|:-----:|-----------:|
| Flurstücke |  land_parcels |         |
| Entzugsleistung 40 m, für 1800 h/a |  geo_poten_40m_with_1800ha | X        |
| Entzugsleistung 40 m, für 2400 h/a |  geo_poten_40m_with_2400ha | X        |
| Entzugsleistung 60 m, für 1800 h/a |  geo_poten_60m_with_1800ha |  X       |
| Entzugsleistung 60 m, für 2400 h/a |  geo_poten_60m_with_2400ha |    X     |
| Entzugsleistung 80 m, für 1800 h/a |  geo_poten_80m_with_1800ha |     X    |
| Entzugsleistung 80 m, für 2400 h/a |  geo_poten_80m_with_2400ha |    X     |
| Entzugsleistung 100 m, für 1800 h/a |  geo_poten_100m_with_1800ha |    X     |
| Entzugsleistung 100 m, für 2400 h/a |  geo_poten_100m_with_2400ha |     X    |
| Geothermisches Potenzial - spezifische Wärmeleitfähigkeit bis 40 m |  thermal_con_40 |    X     |
| Geothermisches Potenzial - spezifische Wärmeleitfähigkeit bis 60 m |  thermal_con_60 |     X    |
| Geothermisches Potenzial - spezifische Wärmeleitfähigkeit bis 80 m |  thermal_con_80 |     X    |
| Geothermisches Potenzial - spezifische Wärmeleitfähigkeit bis 100 m |  thermal_con_100 |    X     |
| Geothermisches Potenzial - Restriktionsflächen |  geo_poten_restrict |    X     |
| Flächen Hauptgrundwasserleiter gespannt |  groundwater_surface_distance |     X   |
| Grundwassergleichen 2020 - Hauptgrundwasserleiter |  ground_water_height_main |    X     |
| Grundwassergleichen 2020 - Panketalgrundwasserleiter |  ground_water_height_tension |   X      |
| Grundwassergüte Ammonium |  water_ammonium |     X    |
| Grundwassergüte Bor |  water_bor |     X    |
| Grundwassergüte Chlor |  water_chlor |     X    |
| Grundwassergüte Elektrische Leitfähigkeit |  electrical_con |    X     |
| Grundwassergüte Kalium |  water_kalium |   X      |
| Grundwassergüte Ortho-Phosphat |  water_ortho_phosphat |      X   |
| Grundwassergüte Sulfat |  water_sulfat |      X   |
| Grundwassermessstellen |  groundwater_measuring_points |     X    |
| Grundwassertemperatur - Durchschnittstemperatur des Grundwasser 20 - 100 m |  mean_water_temp_20to100 |    X     |
| Grundwassertemperatur 20 m unter Geländeoberfläche |  mean_water_temp_20 |     X    |
| Grundwassertemperatur 40 m unter Geländeoberfläche |  mean_water_temp_40 |     X    |
| Grundwassertemperatur 60 m unter Geländeoberfläche |  mean_water_temp_60 |     X    |
| Wasserschutzgebiete |  water_protec_areas |      X   |
| Zu erwartender höchster Grundwasserstand (zeHGW) |  expe_max_groundwater_hight |  X       |


