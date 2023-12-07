namespace GERMAG.DataModel.Database;

public partial class GeothermalParameter
{
    public GeothermalParameter(GeothermalParameter model)
    {
        Getrequest = model.Getrequest;
        Srid = model.Srid;
        LastUpdate = model.LastUpdate;
        LastPing = model.LastPing;
    }

    public GeothermalParameterModel Model()
    {
        return new() { Srid = Srid, LastUpdate = LastUpdate, LastPing = LastPing, Getrequest = Getrequest, Id = Id };
    }
}
