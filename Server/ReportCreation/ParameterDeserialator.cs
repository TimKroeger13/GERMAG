using GERMAG.DataModel.Database;
using GERMAG.Shared;
using System.Text.Json;

namespace GERMAG.Server.ReportCreation;

public interface IParameterDeserialator
{
    Properties DeserializeParameters(string SerializedInputJson);
}

public class ParameterDeserialator : IParameterDeserialator
{
    public Properties DeserializeParameters(string SerializedInputJson)
    {
        return JsonSerializer.Deserialize<Properties>(SerializedInputJson, new JsonSerializerOptions()
        {
            PropertyNameCaseInsensitive = true,
        }) ?? throw new Exception("No wfs found (ParameterRoot)");
    }
}
