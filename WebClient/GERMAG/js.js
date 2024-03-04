proj4.defs("EPSG:4326", "+proj=longlat +datum=WGS84 +no_defs");
proj4.defs("EPSG:25833", "+proj=utm +zone=33 +ellps=GRS80 +units=m +no_defs");

var Current_lat_coordiante = null;
var Current_lng_coordiante = null;

async function onMapClick(e, callback) {

    var clickCoordinates = e.latlng;

    await InitalPointQuery(clickCoordinates.lng, clickCoordinates.lat)

    Current_lat_coordiante = clickCoordinates.lat;
    Current_lng_coordiante = clickCoordinates.lng;
}

async function ShowDetailedReport() {

    var ReportRequest_Json = await GetRequestFullReport();

    if(ReportRequest_Json[0].error != null){
        return true
    }

    const geometry_Usable_json = JSON.parse(ReportRequest_Json[0].geometry_Usable);
    const geometry_Resriction_json = JSON.parse(ReportRequest_Json[0].geometry_Restiction);

    if (geometry_Usable_json.coordinates.length === 0){
        UsabeGeometry = null
    }else{
        var UsabeGeometry = await BackTransformationOfGeometry(ReportRequest_Json[0].geometry_Usable);
    }

    if (geometry_Resriction_json.coordinates.length === 0){
        ResrictionGeometry = null
    }else{
        var ResrictionGeometry = await BackTransformationOfGeometry(ReportRequest_Json[0].geometry_Restiction);
    }

    //Create Gethermalreport
    var GeothermalReport = await CreateReportHTML(ReportRequest_Json[0]);
    await SetReport(GeothermalReport);

    //opens modal window
    await openModal(GeothermalReport);

    await removeLandParcels()
    await CreateLandParcel(UsabeGeometry,'#3388ff','#3388ff',2,1,0.2);

    return true;


}

async function InitalPointQuery(lng, lat) {

    //Get Json from Server
    var ReportRequest_Json = await GetRequest(lng, lat);

    if(ReportRequest_Json[0].error != null){
        //alert(ReportRequest_Json[0].error);
        return true
    }

    //Transform Geometry Back
    var LandParcelGeometry = await BackTransformationOfGeometry(ReportRequest_Json[0].geometry);

    //Create Gethermalreport
    var GeothermalReport = await CreateReportHTML(ReportRequest_Json[0]);
    await SetReport(GeothermalReport);

    //opens modal window
    await openModal(GeothermalReport);

    //Plots geometry on map
    await removeLandParcels()
    await CreateLandParcel(LandParcelGeometry,'#3388ff','#3388ff',2,1,0.2);

    return true

}

async function getGeojsonFromAddress(address) {

    var geoJsonRequest = await httpGet("https://nominatim.openstreetmap.org/search?q=" + address + "&format=geojson");

    var parsedGeoJson = JSON.parse(geoJsonRequest);

    if (parsedGeoJson.features.length == 0) {
        return;
    }

    x = parsedGeoJson.features[0].geometry.coordinates[1];
    y = parsedGeoJson.features[0].geometry.coordinates[0];

    Current_lat_coordiante = x;
    Current_lng_coordiante = y;

    var mapFocus = document.getElementById('map');
    mapFocus.focus();

    var geometryFound = await InitalPointQuery(y, x)

    if (geometryFound) {
        await flyToPoint(x, y)
    }
}

async function openModal(html) {

    $("#myModal").modal({ backdrop: false });
    $('.modal').modal('hide');

    $(".modal-dialog").draggable({
        handle: ".modal-header"
    });

    $("#myModal").on('shown.bs.modal', function (e) {
        $(".modal-body").html(html);
    });
    
    $("#myModal").on('hide.bs.modal', function (e) {
        $(".modal iframe").attr('src', "");
    });
    
    $("#myModal").modal({ backdrop: false });

}


async function PrintPDF() {
    var pdf = new jsPDF('p', 'pt', 'letter');
    var MainReportsource = await GetReport();

    var Title = '<h1 style="margin-bottom: 30px;">Geothermal Report - Gasag Solution Plus</h1>';

    var source = '<div style="font-size: 14px;">' + Title + '<br>' + MainReportsource + '</div>';

    specialElementHandlers = {
        '#bypassme': function (element, renderer) {
            return true;
        }
    };

    margins = {
        top: 40,
        bottom: 60,
        left: 40,
        width: 522
    };

    pdf.fromHTML(
        source,
        margins.left,
        margins.top, {
            'width': margins.width,
            'elementHandlers': specialElementHandlers
        },
        function (dispose) {
            var pdfDataUri = pdf.output('datauristring');
            var newWindow = window.open();
            newWindow.document.write('<style>body, html { margin: 0; padding: 0; }</style>');
            newWindow.document.write('<iframe width="100%" height="100%" style="margin: 0; padding: 0; border: none;" src="' + pdfDataUri + '"></iframe>');
            newWindow.document.title = 'Geothermal-Report';
        }, margins);
}




//Close Modal, when adress bar ist clicked
document.addEventListener('DOMContentLoaded', function () {
    var addressInput = document.getElementById('address-input');
    if (addressInput) {
        addressInput.addEventListener('focus', function () {
            if ($('#myModal').hasClass('in')) {
                $('.modal').modal('hide');
            }
            addressInput.focus();
            closeAllGeometry()
        });
    }
});



function httpGet(theUrl) {
    return new Promise(function (resolve, reject) {
        var xmlHttp = new XMLHttpRequest();
        xmlHttp.open("GET", theUrl, true); // true for asynchronous request

        xmlHttp.onload = function () {
            if (xmlHttp.status >= 200 && xmlHttp.status < 300) {
                resolve(xmlHttp.responseText);
            } else {
                reject(new Error(`HTTP request failed with status ${xmlHttp.status}`));
            }
        };

        xmlHttp.onerror = function () {
            reject(new Error('HTTP request failed'));
        };

        xmlHttp.send();
    });
}


async function CreateReportHTML(reportData) {

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

async function GetRequestFullReport() {
    var Srid = 4326;

    const url = `https://localhost:9999/api/report/fullreport?xCor=${Current_lng_coordiante}&yCor=${Current_lat_coordiante}&srid=${Srid}`;

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


//Quick search by hitting enter
function handleKeyPress(event) {
    if (event.keyCode === 13) {
        searchAddress();
    }
}

// Back transformation

async function BackTransformationOfGeometry(geometry) {

    var LandParcelGeometry = JSON.parse(geometry);

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