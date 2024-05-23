using BulletionBoard.Data.Users;
using BulletionBoard.Services.Hash;
using BulletionBoard.Services.ServiceDependencies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;

namespace BulletionBoard.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {

        private readonly IDbService _dbService;
        private readonly IHashService _hashService;
        private readonly IConfiguration _configuration;

        public AuthController(IHashService hashService, IDbService dbService, IConfiguration configuration)
        {
            _hashService = hashService;
            _dbService = dbService;
            _configuration = configuration;
        }

        [HttpGet]
        public object Authenticate(String login, String password)
        {
            // шукаємо користувача із заданим логіном
            UserEntity user = LoadUserEntityAsync(login).GetAwaiter().GetResult();
            if (user == null)
            {
                HttpContext.Response.StatusCode =
                    StatusCodes.Status401Unauthorized;

                return new { status = "Credentials rejected" };
            }
            // користувач знайдений, формуємо DK з паролю, що надіслано, та
            // солі, що зберігається у БД (як при реєстрації)
            String dk = _hashService.HexString(user.PasswordSalt + password);
            if (user.PasswordDk != dk)
            {
                HttpContext.Response.StatusCode =
                    StatusCodes.Status401Unauthorized;

                return new { status = "Credentials rejected" };
            }
            // зберігаємо у сесії факт успішної автентифікації
            HttpContext.Session.SetString("AuthUserId", user.Id.ToString());
            return new { status = "OK" };
        }

        private async Task <UserEntity> LoadUserEntityAsync(String userLogin)
        {
            CosmosClient cosmosClient = _dbService.GetCosmosClient();
            String dbName = _configuration.GetSection("CosmosDB").GetSection("DbName").Value;
            Database database = await cosmosClient.CreateDatabaseIfNotExistsAsync(dbName);
            Container container = await database.CreateContainerIfNotExistsAsync("Items", "/partitionKey");
            string partitionKey = "User"; // Замените на фактическое значение partition key
            string sqlQueryText = "SELECT * FROM c WHERE c.Login = @login";

            QueryDefinition queryDefinition = new QueryDefinition(sqlQueryText)
                .WithParameter("@login", userLogin);

            FeedIterator<UserEntity> queryResultSetIterator = container
                .GetItemQueryIterator<UserEntity>(queryDefinition, requestOptions: new QueryRequestOptions 
                { PartitionKey = new PartitionKey(partitionKey) });

            UserEntity userEntity = null;

            while (queryResultSetIterator.HasMoreResults)
            {
                FeedResponse<UserEntity> currentResultSet = await queryResultSetIterator.ReadNextAsync();
                foreach (UserEntity entity in currentResultSet)
                {
                    userEntity = entity;
                    break; // Предполагается, что логины уникальны, поэтому можно выйти из цикла после первого найденного совпадения
                }
            }

            if (userEntity == null)
            {
                Console.WriteLine("User not found.");
            }

            return userEntity;
        }
		[HttpDelete]
		public object SignOut()
		{
			// Получаем идентификатор пользователя из сессии
			var userId = HttpContext.Session.GetString("AuthUserId");

			if (userId == null)
			{
				HttpContext.Response.StatusCode = StatusCodes.Status401Unauthorized;
				return new { status = "User not authenticated" };
			}

			// Очищаем сессию для пользователя
			HttpContext.Session.Remove("AuthUserId");

			return new { status = "OK" };
		}


	}
}







