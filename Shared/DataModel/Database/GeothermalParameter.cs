﻿namespace GERMAG.DataModel.Database;

public partial class GeothermalParameter
{
    public GeothermalParameter()
    {
    }

    public int Id { get; set; }
    public string? Getrequest { get; set; }
    public int? Srid { get; set; }
    public DateTime? LastUpdate { get; set; }
    public DateTime? LastPing { get; set; }
}
