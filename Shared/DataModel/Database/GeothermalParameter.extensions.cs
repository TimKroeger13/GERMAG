﻿using NetTopologySuite.Geometries;

namespace GERMAG.DataModel.Database;

public partial class GeothermalParameter
{
    public TypeOfData Type { get; set; }
    public Area Area { get; set; }
    public Range Range { get; set; }
    public Service Service { get; set; }
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
            LastPing = LastPing,
            Type = Type,
            Area = Area,
            Range = Range,
            Service = Service
        };
    }
}