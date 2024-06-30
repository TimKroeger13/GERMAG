proj4.defs("EPSG:4326", "+proj=longlat +datum=WGS84 +no_defs");
proj4.defs("EPSG:25833", "+proj=utm +zone=33 +ellps=GRS80 +units=m +no_defs");

var Current_lat_coordiante = null;
var Current_lng_coordiante = null;

var Multiple_lat = [];
var Multiple_lng = [];

var NewObjectClicked = false;

async function onMapClick(e, callback) {

    Multiple_lng = [];
    Multiple_lat = [];

    NewObjectClicked = true;

    var clickCoordinates = e.latlng;

    await InitalPointQuery(clickCoordinates.lng, clickCoordinates.lat)

}

async function onMapClickWithCtrl(e) {

    NewObjectClicked = true;

    var clickCoordinates = e.latlng;

    Multiple_lng.push(clickCoordinates.lng);
    Multiple_lat.push(clickCoordinates.lat);

    await InitalPointQuery(Multiple_lng, Multiple_lat)
}

async function ShowDetailedReport(reportType) {

    NewObjectClicked = true;

    var ReportRequest_Json = await GetRequestFullReport(reportType);

    if (ReportRequest_Json[0].error != null) {
        alert(ReportRequest_Json[0].error)
        return true
    }

    const geometry_Usable_json = JSON.parse(ReportRequest_Json[0].geometry_Usable);
    const geometry_Resriction_json = JSON.parse(ReportRequest_Json[0].geometry_Restiction);

    if (geometry_Usable_json.coordinates.length === 0) {
        UsabeGeometry = null
    } else {
        var UsabeGeometry = await BackTransformationOfGeometry(ReportRequest_Json[0].geometry_Usable);
    }

    if (geometry_Resriction_json.coordinates.length === 0) {
        ResrictionGeometry = null
    } else {
        var ResrictionGeometry = await BackTransformationOfGeometry(ReportRequest_Json[0].geometry_Restiction);
    }

    if (ReportRequest_Json[0].probePoint.length === 0) {
        ProbePointsGeometry = null
    } else {
        var ProbePointsGeometry = await BackTransformationOfProbepoints(ReportRequest_Json[0].probePoint);
    }

    //Create Gethermalreport
    if(reportType == 'probe'){
        var GeothermalReport = await CreateReportHTML(ReportRequest_Json[0], true, true);
    }else{
        var GeothermalReport = await CreateReportHTML(ReportRequest_Json[0], true, false);
    }

    await SetReport(GeothermalReport);

    //opens modal window

    if(reportType == 'probe'){
        await openModal(GeothermalReport, true, ReportRequest_Json[0]);
    }else{
        await openModal(GeothermalReport, false);
    }

    await removeLandParcels()
    await CreateLandParcel(UsabeGeometry, '#00ff00', '#00ff00', 2, 0, 0.2);  //2,0,0.2
    await CreateLandParcel(ResrictionGeometry, '#ff6600', '#ff6600', 2, 1, 0.2);

    NewObjectClicked = false;

    if (ReportRequest_Json[0].probePoint.length != 0) {
        for (let k = 0; k < ProbePointsGeometry.length; k++) {
            if (NewObjectClicked){
                break
            }

            //await new Promise(resolve => setTimeout(resolve, 5)); // Introduce a delay of 100 milliseconds
            await CreatePoint(ProbePointsGeometry[k].coordinates[0]);
        }
    }

    return true;
}

//multiple buttons

async function InitalPointQuery(lng, lat) {

    if (!Array.isArray(lng) && !Array.isArray(lng) ){

        lng = [lng]
        lat = [lat]

    }

    Current_lat_coordiante = lat;
    Current_lng_coordiante = lng;

    //Get Json from Server
    var ReportRequest_Json = await GetRequest(lng, lat);

    if (ReportRequest_Json[0].error != null) {
        Multiple_lat.pop();
        Multiple_lng.pop();
        alert(ReportRequest_Json[0].error);
        return true
    }

    //Transform Geometry Back
    var LandParcelGeometry = await BackTransformationOfGeometry(ReportRequest_Json[0].geometry);

    //Create Gethermalreport
    var GeothermalReport = await CreateReportHTML(ReportRequest_Json[0], false, false);
    await SetReport(GeothermalReport);

    //opens modal window
    await openModal(GeothermalReport, false);

    //Plots geometry on map
    await removeLandParcels()
    await CreateLandParcel(LandParcelGeometry, '#3388ff', '#3388ff', 2, 1, 0.2);

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

async function openModal(html, ReportIsDetailed, jsonData) {
    $("#myModal").modal({ backdrop: false });
    $('.modal').modal('hide');

    $(".modal-dialog").draggable({
        handle: ".modal-header"
    });

    $(".modal-body").html(html);

    $("#myModal").off('shown.bs.modal');

    if (ReportIsDetailed) {
        await loadGoogleCharts();
        $("#myModal").on('shown.bs.modal', function (e) {
            drawChart(jsonData);
        });
    }

    $("#myModal").on('hide.bs.modal', function (e) {
        $(".modal iframe").attr('src', "");
    });

    $("#myModal").modal({ backdrop: false });
}

async function loadGoogleCharts() {
    return new Promise((resolve, reject) => {
        var script = document.createElement('script');
        script.src = 'https://www.gstatic.com/charts/loader.js?version={version}';
        document.head.appendChild(script);

        script.onload = function () {
            google.charts.load('current', { 'packages': ['corechart'] });
            google.charts.setOnLoadCallback(resolve); 
        };

        script.onerror = reject;
    });
}

function drawChart(JasonData) {

    var classifedData = classifyData(JasonData)

    const dataArray = Object.entries(classifedData).map(([task, hours]) => [task.toString(), hours]);

    dataArray.unshift(['Task', 'Hours per Day']);

    var data = google.visualization.arrayToDataTable(dataArray);

    var options = {
        title: "Proben genaue Auflösung"
    };

    var container = document.getElementById('piechart'); 
    if (container) {
        var chart = new google.visualization.PieChart(container);
        chart.draw(data, options);
    } else {
        console.error("Container for chart not found");
    }
}

function classifyData(JasonData) {
    const counts = {};

    for (let i = 0; i < JasonData.probePoint.length; i++) {
        const geoPoten = JasonData.probePoint[i].properties.geoPoten;
        if (counts[geoPoten]) {
            counts[geoPoten]++;
        } else {
            counts[geoPoten] = 1;
        }
    }

    return counts;
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
            removeLandParcels();
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


async function CreateReportHTML(reportData, ReportIsDetailed, DisplayGoogleGrafics) {

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

    holstein = ``;
    if(reportData.holstein != ""){
        holstein = `<p><strong>Holstein-Schicht:</strong> ${reportData.holstein} meter</p>`
    }

    var html = `
    <div class="geothermal-report">
        <p><strong>Gemeinde:</strong> ${reportData.land_parcels_gemeinde}</p>
        <p><strong>Flurstück:</strong> ${reportData.land_parcel_number}</p>
        <p><strong>Entzugsleistungen 2400ha (W/m):</strong></p>
        <ul>
            <li><strong>100:</strong> ${reportData.geo_poten_100m_with_2400ha}</li>
            <li><strong>80:</strong> ${reportData.geo_poten_80m_with_2400ha}</li>
            <li><strong>60:</strong> ${reportData.geo_poten_60m_with_2400ha}</li>
            <li><strong>40:</strong> ${reportData.geo_poten_40m_with_2400ha}</li>
        </ul>
        <p><strong>Entzugsleistungen 1800ha (W/m):</strong></p>
        <ul>
            <li><strong>100:</strong> ${reportData.geo_poten_100m_with_1800ha}</li>
            <li><strong>80:</strong> ${reportData.geo_poten_80m_with_1800ha}</li>
            <li><strong>60:</strong> ${reportData.geo_poten_60m_with_1800ha}</li>
            <li><strong>40:</strong> ${reportData.geo_poten_40m_with_1800ha}</li>
        </ul>
        <p><strong>Spezifische Wärmeleitfähigkeit (Wm<sup>-1</sup>K<sup>-1</sup>):</strong></p>
        <ul>
            <li><strong>100:</strong> ${reportData.thermal_con_100}</li>
            <li><strong>80:</strong> ${reportData.thermal_con_80}</li>
            <li><strong>60:</strong> ${reportData.thermal_con_60}</li>
            <li><strong>40:</strong> ${reportData.thermal_con_40}</li>
        </ul>
        <p><strong>Grundwassertemperatur unter Geländeoberfläche (°C):</strong></p>
        <ul>
            <li><strong>20to100:</strong> ${reportData.mean_water_temp_20to100}</li>
            <li><strong>60:</strong> ${reportData.mean_water_temp_60}</li>
            <li><strong>40:</strong> ${reportData.mean_water_temp_40}</li>
            <li><strong>20:</strong> ${reportData.mean_water_temp_20}</li>
        </ul>
        ${holstein}`;

    if (ReportIsDetailed) {

        //Expected groundwater hight
        html = html + `
        <p><strong>ZeHGW:</strong> ${reportData.zeHGW} meter</p>`

        //Usable Area
        html = html + `
    <p><strong>Usable Area:</strong> ${Math.round(reportData.usable_Area * 100) / 100}m&sup2</p>`
        //Number Of Probes
        html = html + `
    <p><strong>Amount of possible probes:</strong> ${reportData.probePoint.length}</p>`

    html = html + `
    ${String_geo_poten_restrict}
    ${String_Water_protec_areas}`

    }

    if(DisplayGoogleGrafics) {
        html = html + `<div id="piechart" style="width: 500px; height: 300px;"></div>`
    }

    html = html + `
        
</div>`

    return html

}


async function GetRequest(XcorList, YcorList) {
    var Srid = 4326;

    const params = new URLSearchParams();
    XcorList.forEach(x => params.append('xCor', x));
    YcorList.forEach(y => params.append('yCor', y));
    params.append('srid', Srid);

    const url = `https://localhost:9999/api/report/reportdata?${params.toString()}`;

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

async function GetRequestFullReport(reportType) {
    var Srid = 4326;

    const params = new URLSearchParams();
    Current_lng_coordiante.forEach(x => params.append('xCor', x));
    Current_lat_coordiante.forEach(y => params.append('yCor', y));
    params.append('srid', Srid);

    if(reportType == 'probe'){
        url = `https://localhost:9999/api/report/fullreport?${params.toString()}&probeRes=true`;
    }else{
        url = `https://localhost:9999/api/report/fullreport?${params.toString()}&probeRes=false`;
    }

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

    if (LandParcelGeometry.type == "Polygon") {

        if (Array.isArray(LandParcelGeometry.coordinates) && LandParcelGeometry.coordinates.length > 0) {

            if (LandParcelGeometry.coordinates.length == 1) {

                var flattenedCoordinates = transformCoordinates(LandParcelGeometry.coordinates[0]);

                LandParcelGeometry.coordinates[0] = flattenedCoordinates;

            } else {
                for (let i = 0; i < LandParcelGeometry.coordinates.length; i++) {

                    if (LandParcelGeometry.coordinates[0][0].length == 2) {
                        var flattenedCoordinates = transformCoordinates(LandParcelGeometry.coordinates[i]);
                        LandParcelGeometry.coordinates[i] = flattenedCoordinates;
                    } else {
                        var flattenedCoordinates = transformCoordinates(LandParcelGeometry.coordinates[i][0]);
                        LandParcelGeometry.coordinates[i][0] = flattenedCoordinates;
                    }

                }
            }

            return LandParcelGeometry;

        } else {
            console.error("Error: Invalid coordinates format in LandParcelGeometry");
        }
    }

    if (LandParcelGeometry.type == "MultiPolygon") {
        if (Array.isArray(LandParcelGeometry.coordinates) && LandParcelGeometry.coordinates.length > 0) {
            for (let i = 0; i < LandParcelGeometry.coordinates.length; i++) {
                for (let j = 0; j < LandParcelGeometry.coordinates[i].length; j++) {
                    var flattenedCoordinates = transformCoordinates(LandParcelGeometry.coordinates[i][j]);
                    LandParcelGeometry.coordinates[i][j] = flattenedCoordinates;
                }
            }
            return LandParcelGeometry;
        } else {
            console.error("Error: Invalid coordinates format in MultiPolygon");
        }
    }
}



async function BackTransformationOfProbepoints(probePoints) {

    var cor = []

    for (let i = 0; i < probePoints.length; i++) {
        var probePoint = probePoints[i];

        var geometry = JSON.parse(probePoint.geometryJson);

        probePoint.coordinates = transformCoordinates([geometry.coordinates]);

        probePoints[i] = probePoint;

        //var transformedCoordinates = transformCoordinates(geometry.coordinates[0]);
        //cor[i] = transformedCoordinates;

    }
    //probePoint.geometryJson = cor;
    return probePoints;
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