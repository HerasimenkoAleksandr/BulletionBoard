using Microsoft.Azure.Cosmos;

namespace BulletionBoard.Properties.ServiceDependencies
{
    public interface IDbService
    {
        CosmosClient GetCosmosClient();
        Task<Database> GetDatabaseAsync(string databaseName);
    }
}
