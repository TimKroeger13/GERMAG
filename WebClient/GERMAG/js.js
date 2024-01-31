proj4.defs("EPSG:4326", "+proj=longlat +datum=WGS84 +no_defs");
proj4.defs("EPSG:25833", "+proj=utm +zone=33 +ellps=GRS80 +units=m +no_defs");

async function onMapClick(e) {

    var clickCoordinates = e.latlng;

    var transformedCoordinates = proj4("EPSG:4326", "EPSG:25833", [clickCoordinates.lng, clickCoordinates.lat]);

    var ReportRequest = await GetRequest(transformedCoordinates[0], transformedCoordinates[1]);

    await CreatPopUp(clickCoordinates,ReportRequest);

    var LandParvelGeometry = JSON.parse(ReportRequest[0].geometry)

    //return LandParvelGeometry;
}

async function GetRequest(Xcor, Ycor) {
    var Srid = 25833;

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