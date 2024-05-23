using BulletionBoard.Data.Users;
using BulletionBoard.Services.ServiceDependencies;
using Microsoft.Azure.Cosmos;
using System.Security.Claims;

namespace BulletionBoard.Middleware
{
    public class AuthSessionMiddleware
    {
        // ланцюг Middleware утворюється при інсталяції проєкту
        // і кожен учасник (ланка) Middleware одержує при створенні
        // посилання на наступну ланку (_next). Підключення Middleware
        // здійснюється у Program.cs
        private readonly RequestDelegate _next;

        public AuthSessionMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        // є дві схеми включення Middleware - синхронна та асинхронна
        // Для них передбачені методи Invoke або InvokeAsync
        public async Task InvokeAsync(HttpContext context,
            IDbService _dbService, IConfiguration _configuration// інжекція у Middleware іде через метод
        )
        {
            // Задача - перевірити наявність у сесії збереженого AuthUserId
            // за наявності - перевірити валідність шляхом пошуку у БД
            if (context.Session.Keys.Contains("AuthUserId"))
            {

                CosmosClient cosmosClient = _dbService.GetCosmosClient();
                string dbName = _configuration.GetSection("CosmosDB").GetSection("DbName").Value;
                Database database = await cosmosClient.CreateDatabaseIfNotExistsAsync(dbName);
                Container container = await database.CreateContainerIfNotExistsAsync("Items", "/partitionKey");

                string userId = context.Session.GetString("AuthUserId")!; // Замените на фактический ID пользователя
                string partitionKey = "User"; // Замените на фактическое значение partition key      
                   
                ItemResponse<UserEntity> response = await container.ReadItemAsync<UserEntity>(userId, new PartitionKey(partitionKey));
                UserEntity user = response.Resource;

                
                if (user != null)
                {
                    // перекладаємо відомості про користувача до 
                    // контексту НТТР у формалізмі Claims
                    Claim[] claims = new Claim[]
                    {
                        new Claim(ClaimTypes.Sid, user.Id.ToString()),
                        new(ClaimTypes.Name, user.Name ?? ""),
                        new(ClaimTypes.Email, user.Email),
                        new(ClaimTypes.UserData, user.Avatar ?? ""),
                        new Claim(ClaimTypes.MobilePhone, user.Phone ?? ""), 
                         new Claim(ClaimTypes.StreetAddress, user.Address ?? "")
                    };
                    context.User = new ClaimsPrincipal(
                        new ClaimsIdentity(
                            claims,
                            nameof(AuthSessionMiddleware)
                        )
                    );
                }
            }
            // тіло Middleware ділиться на дві частини:
            // "прямий" хід (до виклику наступної ланки) ...
            await _next(context);
            // ... та зворотній хід - після виклику.
        }
    }
}
