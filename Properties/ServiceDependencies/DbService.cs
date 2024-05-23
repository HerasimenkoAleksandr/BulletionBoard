using Microsoft.Azure.Cosmos;

namespace BulletionBoard.Properties.ServiceDependencies
{
    public class DbService : IDbService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<DbService> _logger;
        private readonly Lazy<CosmosClient> _cosmosClient;

        public DbService(IConfiguration configuration, ILogger<DbService> logger)
        {
            _configuration = configuration;
            _cosmosClient = new Lazy<CosmosClient>(InitializeCosmosClient, true);
            _logger = logger;
        }

        public CosmosClient GetCosmosClient()
        {
            _logger.LogInformation("Returning CosmosClient instance.");
            return _cosmosClient.Value;
        }

        public async Task<Database> GetDatabaseAsync(string databaseName)
        {
            CosmosClient client = GetCosmosClient();
            _logger.LogInformation("Creating or getting database: {databaseName}", databaseName);
            return await client.CreateDatabaseIfNotExistsAsync(databaseName);
        }

        private CosmosClient InitializeCosmosClient()
        {
            string cosmosEndpoint = _configuration.GetSection("CosmosDB").GetSection("Endpoint").Value;
            string cosmosKey = _configuration.GetSection("CosmosDB").GetSection("Key").Value;
            string applicationName = _configuration.GetSection("CosmosDB").GetSection("ApplicationName").Value;


            _logger.LogInformation("Initializing CosmosClient.");

            return new CosmosClient(cosmosEndpoint, cosmosKey, new CosmosClientOptions
            {
                ApplicationName = applicationName
            });
        }
    }
}
