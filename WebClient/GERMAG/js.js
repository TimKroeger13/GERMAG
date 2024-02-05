proj4.defs("EPSG:4326", "+proj=longlat +datum=WGS84 +no_defs");
proj4.defs("EPSG:25833", "+proj=utm +zone=33 +ellps=GRS80 +units=m +no_defs");

async function onMapClick(e, callback) {
    var clickCoordinates = e.latlng;

    //Get Json from Server
    var ReportRequest_Json = await GetRequest(clickCoordinates.lng, clickCoordinates.lat);

    //Transform Geometry Back
    var LandParcelGeometry = await BackTransformationOfGeometry(ReportRequest_Json);

    //Creates PDF
    //generatePDF(ReportRequest_Json[0])
    generateReport(ReportRequest_Json[0])

    //Creates Pop up data of object
    await CreatPopUp(clickCoordinates, ReportRequest_Json);

    //Plots geometry on map
    callback(LandParcelGeometry);

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

async function CreatPopUp(clickCoordinates,ReportRequest){
    const reportData = ReportRequest[0];

    const popupContent = `
    <div class="geothermal-report">
        <h3>Geothermal Report:</h3>
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
    </div>
`;

    popup
        .setLatLng(clickCoordinates)
        .setContent(popupContent)
        .openOn(map);

}

/*
async function generatePDF(data) {
    // Get the data from your JSON
    const thermalCon100 = data.thermal_con_100;
    const thermalCon80 = data.thermal_con_80;

    // Create a new jsPDF instance
    const pdf = new window.jspdf.jsPDF();

    // Add content to the PDF
    pdf.text('Random Text:', 10, 10);
    pdf.text('Some random content here...', 10, 20);

    // Add thermal_con_100 and thermal_con_80
    pdf.text('thermal_con_100:', 10, 30);
    pdf.text(thermalCon100, 10, 40);

    pdf.text('thermal_con_80:', 10, 50);
    pdf.text(thermalCon80, 10, 60);

    // Convert the PDF to a Blob
    const pdfBlob = await pdf.output('blob');

    // Create a Blob URL
    const pdfUrl = URL.createObjectURL(pdfBlob);

    // Open the PDF in a new window
    window.open(pdfUrl, '_blank');
}
*/

function generateReport(reportData) {

const reportContent = `
    <div class="geothermal-report">
        <h3>Geothermal Report:</h3>
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
    </div>
`;

    // Calculate center coordinates
    const screenWidth = window.innerWidth || document.documentElement.clientWidth || document.body.clientWidth;
    const screenHeight = window.innerHeight || document.documentElement.clientHeight || document.body.clientHeight;

    const popWidth = 600;
    const popHeight = 700;

    //const left = (screenWidth - popWidth) / 2;
    //const top = (screenHeight - popHeight) / 2;

    // Open a new window in the middle of the screen with the report content
    //const popupWindow = window.open('', 'ReportWindow', `width=${popWidth},height=${popHeight},left=${left},top=${top}`);
    const popupWindow = window.open('', 'ReportWindow', `width=${popWidth},height=${popHeight},left=${50},top=${100}`);
    //const popupWindow = window.open('', 'ReportWindow');

    // Write the report content to the new window
    popupWindow.document.write(`
        <!DOCTYPE html>
        <html>
        <head>
            <title>Report</title>
            <link rel="stylesheet" href="styles.css"> 
        </head>
        <body>
            <div>${reportContent}</div>
        </body>
        </html>
    `);

    // Close the document stream (important for IE compatibility)
    popupWindow.document.close();
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