namespace GERMAG.Server.Core.Configurations
{
    public interface IEnviromentConfiguration
    {
        public string DatabaseConnection { get; }
    }

    public class DebugConfiguration : IEnviromentConfiguration
    {
        private readonly ConfigurationOptions _options;

        public DebugConfiguration(ConfigurationOptions options)
        {
            _options = options;
        }

        public string DatabaseConnection => _options.DatabaseConnection;
    }
    public class ReleaseConfiguration : IEnviromentConfiguration
    {
        private readonly ConfigurationOptions _options;

        public ReleaseConfiguration(ConfigurationOptions options)
        {
            _options = options;
        }

        public string DatabaseConnection => Environment.GetEnvironmentVariable(_options.DatabaseConnection) ??
            throw new Exception("Databaseconnection not found");
    }

}
