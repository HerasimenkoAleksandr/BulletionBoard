using Microsoft.Azure.Cosmos;

namespace BulletionBoard.Services.ServiceDependencies
{
    public interface IDbService
    {
        CosmosClient GetCosmosClient();
        Task<Database> GetDatabaseAsync(string databaseName);
    }
}
