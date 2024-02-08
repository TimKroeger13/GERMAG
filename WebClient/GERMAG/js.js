proj4.defs("EPSG:4326", "+proj=longlat +datum=WGS84 +no_defs");
proj4.defs("EPSG:25833", "+proj=utm +zone=33 +ellps=GRS80 +units=m +no_defs");

async function onMapClick(e, callback) {
    var clickCoordinates = e.latlng;

    //Get Json from Server
    var ReportRequest_Json = await GetRequest(clickCoordinates.lng, clickCoordinates.lat);

    //Transform Geometry Back
    var LandParcelGeometry = await BackTransformationOfGeometry(ReportRequest_Json);

    //opens modal window
    await openModal(ReportRequest_Json[0])

    //Test
    await getGeojsonFromAddress('Silbersteinstraße 80 Berlin')

    //Plots geometry on map
    callback(LandParcelGeometry);

}

async function openModal(reportData) {

    var html = CreateReportHTML(reportData)

    $("#myModal").modal({ backdrop: false });
    $('.modal').modal('hide');

    $(".modal-dialog").draggable({
        handle: ".modal-header"
    });

    $(".modal").on('shown.bs.modal', function (e) {
        $(".modal-body").html(html);
    });

    $(".modal").on('hide.bs.modal', function (e) {
        $(".modal iframe").attr('src', "");
    });

    $("#myModal").modal({ backdrop: false });
    // Show overlay
    $('.modal-overlay').show();

    $('#myModal').modal('show');
}



async function getGeojsonFromAddress(addres) {

    var geoJsonRequest = await httpGet("https://nominatim.openstreetmap.org/search?q=" + addres + "&format=geojson");

    var parsedGeoJson = JSON.parse(geoJsonRequest);

    x = parsedGeoJson.features[0].geometry.coordinates[1];
    y = parsedGeoJson.features[0].geometry.coordinates[0];

    console.log("X = ", x, "\n", "Y = ", y, "\n\n");

}

async function httpGet(theUrl)
{
    var xmlHttp = new XMLHttpRequest();
    xmlHttp.open( "GET", theUrl, false ); // false for synchronous request
    xmlHttp.send( null );
    return xmlHttp.responseText;
}




function CreateReportHTML(reportData) {

    String_geo_poten_restrict = ``;
    if (reportData.geo_poten_restrict.length > 0) {
        for (let i = 0; i < reportData.geo_poten_restrict.length; i++) {
            String_geo_poten_restrict = String_geo_poten_restrict + `<p><strong>Restriktionsflächen:</strong> ${reportData.geo_poten_restrict[i]}</p>`
        }
    }

    //Get Unique Elemets
    reportData.verordnung = [...new Set(reportData.verordnung)]
    reportData.veror_link = [...new Set(reportData.veror_link)]

    String_Water_protec_areas = ``;
    if (reportData.verordnung.length > 0) {
        for (let i = 0; i < reportData.verordnung.length; i++) {
            String_Water_protec_areas = String_Water_protec_areas + `<p><strong>Wasserschutzgebiet:</strong><a href="${reportData.veror_link[i]}">${reportData.verordnung[i]}</a></p>`
        }
    }

    const html = `
    <div class="geothermal-report">
        <p><strong>Gemeinde:</strong> ${reportData.land_parcels_gemeinde}</p>
        <p><strong>Flurstück:</strong> ${reportData.land_parcel_number}</p>
        <p><strong>Entzugsleistungen 2400ha:</strong></p>
        <ul>
            <li><strong>100:</strong> ${reportData.geo_poten_100m_with_2400ha}</li>
            <li><strong>80:</strong> ${reportData.geo_poten_80m_with_2400ha}</li>
            <li><strong>60:</strong> ${reportData.geo_poten_60m_with_2400ha}</li>
            <li><strong>40:</strong> ${reportData.geo_poten_40m_with_2400ha}</li>
        </ul>
        <p><strong>Entzugsleistungen 1800ha:</strong></p>
        <ul>
            <li><strong>100:</strong> ${reportData.geo_poten_100m_with_1800ha}</li>
            <li><strong>80:</strong> ${reportData.geo_poten_80m_with_1800ha}</li>
            <li><strong>60:</strong> ${reportData.geo_poten_60m_with_1800ha}</li>
            <li><strong>40:</strong> ${reportData.geo_poten_40m_with_1800ha}</li>
        </ul>
        <p><strong>Spezifische Wärmeleitfähigkeit:</strong></p>
        <ul>
            <li><strong>100:</strong> ${reportData.thermal_con_100}</li>
            <li><strong>80:</strong> ${reportData.thermal_con_80}</li>
            <li><strong>60:</strong> ${reportData.thermal_con_60}</li>
            <li><strong>40:</strong> ${reportData.thermal_con_40}</li>
        </ul>
        <p><strong>Grundwassertemperatur unter Geländeoberfläche:</strong></p>
        <ul>
            <li><strong>20to100:</strong> ${reportData.mean_water_temp_20to100}</li>
            <li><strong>60:</strong> ${reportData.mean_water_temp_60}</li>
            <li><strong>40:</strong> ${reportData.mean_water_temp_40}</li>
            <li><strong>20:</strong> ${reportData.mean_water_temp_20}</li>
        </ul>
        <!-- <p><strong>zeHGW:</strong> ${reportData.zeHGW}</p> -->
        ${String_geo_poten_restrict}
        ${String_Water_protec_areas}
        
    </div>
`;

    return html

}


async function GetRequest(Xcor, Ycor) {
    var Srid = 4326;

    const url = `https://localhost:9999/api/report/reportdata?xCor=${Xcor}&yCor=${Ycor}&srid=${Srid}`;

    try {
        const response = await fetch(url);

        if (!response.ok) {
            console.error('Server error:', response.status);

            try {
                const errorData = await response.json();
                console.error('Error details:', errorData);
                alert(`Server error: ${response.status} - ${errorData.message}`);
            } catch (parseError) {
                console.error('Error parsing JSON:', parseError);
                alert(`Server error: ${response.status} - Error parsing response`);
            }

            return null;
        }

        const GetJsonString = await response.json();
        return GetJsonString;

    } catch (error) {
        console.error('Error:', error.message);
        alert('Network error or failed request. Please try again.');
        return null;
    }
}

// Back transformation

async function BackTransformationOfGeometry(ReportRequest) {

    var LandParcelGeometry = JSON.parse(ReportRequest[0].geometry);

    if (Array.isArray(LandParcelGeometry.coordinates) && LandParcelGeometry.coordinates.length > 0) {
        var flattenedCoordinates = transformCoordinates(LandParcelGeometry.coordinates[0]);

        LandParcelGeometry.coordinates[0] = flattenedCoordinates;

        return LandParcelGeometry;

    } else {
        console.error("Error: Invalid coordinates format in LandParcelGeometry");
    }
}

function transformCoordinates(coordinates) {
    var transformedCoordinates = [];

    for (var i = 0; i < coordinates.length; i++) {
        var pair = coordinates[i];

        if (Array.isArray(pair) && pair.length === 2) {
            transformedCoordinates.push(proj4("EPSG:25833", "EPSG:4326", pair));
        } else {
            console.error("Error: Invalid pair format in LandParcelGeometry");
            return [];
        }
    }

    return transformedCoordinates;
}