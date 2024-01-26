using GERMAG.DataModel.Database;
using GERMAG.Shared;
using System.Text.Json;

namespace GERMAG.Server.ReportCreation;

public interface IParameterDeserialator
{
    ParameterRoot DeserializeParameters(string SerializedInputJson);
}

public class ParameterDeserialator : IParameterDeserialator
{
    public ParameterRoot DeserializeParameters(string SerializedInputJson)
    {
        return JsonSerializer.Deserialize<ParameterRoot>(SerializedInputJson, new JsonSerializerOptions()
        {
            PropertyNameCaseInsensitive = true,
        }) ?? throw new Exception("No wfs found (ParameterRoot)");
    }
}
