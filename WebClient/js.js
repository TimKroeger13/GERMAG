async function GetRequest() {
    document.getElementById("GetRequestString").innerHTML = "Getting request...";

    const response = await fetch("https://localhost:9999/api/report/reportdata");
    const GetJsonString = await response.text();

    const obj = JSON.parse(GetJsonString);
    
    document.getElementById("GetRequestString").innerHTML = GetJsonString;
    document.getElementById("SingleResult").innerHTML = obj[0].test;
}




