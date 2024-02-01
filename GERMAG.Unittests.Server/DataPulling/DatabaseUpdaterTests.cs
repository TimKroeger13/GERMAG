using GERMAG.DataModel.Database;
using GERMAG.Server.Core.Configurations;
using GERMAG.Server.DataPulling;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace GERMAG.Unittests.Server.DataPulling;

public class DatabaseUpdaterTests
{
    private readonly DataContext _subDataContext;

    public DatabaseUpdaterTests()
    {
        var json = File.ReadAllText("../../../../Server/appsettings.Development.json");
        var appsettings = JsonNode.Parse(json)?[nameof(ConfigurationOptions.Configuration)]?.ToString() ??
            throw new Exception("appsettings not found");
        var configuration = JsonSerializer.Deserialize<ConfigurationOptions>(appsettings);
        _subDataContext = new DataContext(configuration?.DatabaseConnection ?? "");
    }

    private DatabaseUpdater CreateDatabaseUpdater()
    {
        return new DatabaseUpdater(_subDataContext);
    }

    [Fact]
    public void UpdateDatabase_ShouldWork()
    {
        var sut = CreateDatabaseUpdater();
        int foreignKey = 1;
        sut.UpdateDatabase(new(), foreignKey);
    }
}