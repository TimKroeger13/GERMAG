/*async function GetRequest() {
    document.getElementById("GetRequestString").innerHTML = "Getting request...";

    const response = await fetch("https://localhost:9999/api/report/reportdata");
    const GetJsonString = await response.text();

    const obj = JSON.parse(GetJsonString);
    
    document.getElementById("GetRequestString").innerHTML = GetJsonString;
    //document.getElementById("SingleResult").innerHTML = obj[0].test;
}*/


async function GetRequest() {
    document.getElementById("GetRequestString").innerHTML = "Getting request...";

    const response = await fetch("https://localhost:9999/api/report/reportdata");
    const GetJsonString = await response.text();

    const obj = JSON.parse(GetJsonString);

    document.getElementById("GetRequestString").innerHTML = GetJsonString;

    // Open a new pop-up window
    const popup = window.open("", "_blank", "width=" + screen.width + ",height=" + screen.height);

    // HTML content for the pop-up window
    let popupContent = "<h2>API Response</h2>";

    // Iterate through each property in the JSON object and add to the HTML content
    for (const key in obj[0]) {
        const value = obj[0][key];
        popupContent += `<p><strong>${key}:</strong> ${value}</p>`;
    }

    // Set the HTML content of the pop-up window
    popup.document.body.innerHTML = popupContent;
}

