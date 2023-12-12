namespace GERMAG.DataModel;
using System.ComponentModel.DataAnnotations;

public class GeothermalParameterModel
{
    public int Id { get; set; }

    [Url]
    public string? Getrequest { get; set; }

    [Range(0, int.MaxValue)]
    public int? Srid { get; set; }

    public DateTime? LastUpdate { get; set; }

    public DateTime? LastPing { get; set; }
}
