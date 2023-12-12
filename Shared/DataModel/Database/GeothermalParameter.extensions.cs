namespace GERMAG.DataModel.Database;

public partial class GeothermalParameter
{
    public GeothermalParameter(GeothermalParameterModel model)
    {
        Getrequest = model.Getrequest;
        Srid = model.Srid;
        LastUpdate = model.LastUpdate;
        LastPing = model.LastPing;
    }

    public GeothermalParameterModel Model()
    {
        return new()
        {
            Srid = Srid,
            LastUpdate = LastUpdate,
            Getrequest = Getrequest,
            Id = Id,
            LastPing = LastPing
        };
    }
}