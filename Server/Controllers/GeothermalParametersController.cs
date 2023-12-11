using GERMAG.DataModel;
using GERMAG.DataModel.Database;
using Microsoft.AspNetCore.Mvc;

namespace GERMAG.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GeothermalParameterController(DataContext dataContext)
{
    private readonly DataContext _dataContext = dataContext;

    [HttpGet("parameter")]
    public IEnumerable<GeothermalParameterModel> GetParameters()
    {
        return _dataContext.GeothermalParameters.Select(p => p.Model());
    }

    [HttpPost("addparameter")]
    public void AddParameter(GeothermalParameterModel model)
    {
        _dataContext.GeothermalParameters.Add(new GeothermalParameter(model));
        _dataContext.SaveChanges();
    }

    [HttpDelete("deleteparameter")]
    public void DeleteParameter(long id)
    {
        _dataContext.GeothermalParameters.Remove(_dataContext.GeothermalParameters.First(p => p.Id == id));
        _dataContext.SaveChanges();
    }

    [HttpPut("updateparameter")]
    public void UpdateParameter(GeothermalParameterModel model)
    {
        var dbModel = _dataContext.GeothermalParameters.First(p => p.Id == model.Id);
        dbModel.LastUpdate = model.LastUpdate;
        dbModel.LastPing = model.LastPing;
        _dataContext.SaveChanges();
    }
}