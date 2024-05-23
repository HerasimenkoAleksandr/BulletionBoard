using BulletionBoard.Models;
using BulletionBoard.Models.PicturesHome;
using BulletionBoard.Data.Users;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using static System.Net.Mime.MediaTypeNames;
using System.Reflection;
using System.Collections.Generic;
using BulletionBoard.Models.Home;
using System.Text.Json;
using Microsoft.Azure.Cosmos;
using BulletionBoard.Services.ServiceDependencies;
using BulletionBoard.Services.Hash;
using System.Security.Claims;
using System.Security.Cryptography;
using BulletionBoard.Data.Ads;
using System.Text.RegularExpressions;




namespace BulletionBoard.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IConfiguration _configuration;
        private readonly IDbService _dbService;
        private readonly IHashService _hashService;
        public HomeController(ILogger<HomeController> logger, IConfiguration configuration, IDbService dbService, IHashService hashService)
        {
            _logger = logger;
            _configuration = configuration;
            _dbService = dbService;
            _hashService = hashService;
        }

     

      
        public async Task<ViewResult> ProfileAsync(String? id)
        {
            UserProfileViewModel model = new();
            // Шукаємо у контексті даних (у БД)
            /*CosmosClient cosmosClient = _dbService.GetCosmosClient();
			string dbName = _configuration.GetSection("CosmosDB").GetSection("DbName").Value;
			Database database = await cosmosClient.CreateDatabaseIfNotExistsAsync(dbName);
			Container container = await database.CreateContainerIfNotExistsAsync("Items", "/partitionKey");*/
            if(id != null)
            { model.User = await LoadUserByIdAsync(id); }
            String sid = null!; ;
            if (HttpContext.User.Identity?.IsAuthenticated ?? false)
            {
                model.IsPersonal = true;
                // шукаємо дані користувача за Claim
               sid = HttpContext
                    .User
                    .Claims
                    .First(claim => claim.Type == ClaimTypes.Sid)
                    .Value;
            }
                if (id==sid)
                {
                    model.IsPersonal = true;
                    
                }
                else  // спроба доступу без входу у систему
                {
                    model.IsPersonal = false;
                }

          
            return View(model);
        }

    

        public IActionResult Index()
        {
            IndexViewModel model = new();
            return View(model);

        }
        public async Task<IActionResult> CosmosAsync()
        {
            SignupViewModel model = new();
            model.userEntities = new();
            CosmosClient cosmosClient = _dbService.GetCosmosClient();
            string dbName = _configuration.GetSection("CosmosDB").GetSection("DbName").Value;
            Database database = await cosmosClient.CreateDatabaseIfNotExistsAsync(dbName);
            Container container = await database.CreateContainerIfNotExistsAsync("Items", "/partitionKey");

            string partitionKey = "User"; // Замените на фактическое значение partition key
            string sqlQueryText = "SELECT * FROM c WHERE c.partitionKey = @partitionKey";

            QueryDefinition queryDefinition = new QueryDefinition(sqlQueryText)
                .WithParameter("@partitionKey", partitionKey);

            FeedIterator<UserEntity> queryResultSetIterator = container.GetItemQueryIterator<UserEntity>(queryDefinition);

            while (queryResultSetIterator.HasMoreResults)
            {
                FeedResponse<UserEntity> currentResultSet = await queryResultSetIterator.ReadNextAsync();
                foreach (UserEntity entity in currentResultSet)
                {
                    model.userEntities.Add(entity);
                }
            }

            return View(model);
        }


        public ViewResult Sіgnup()
        {
			SignupViewModel viewModel = new();

            // перевіряємо, чи є дані від форми
            if (HttpContext.Session.Keys.Contains("formStatus"))
            {
                // декодуємо статус
                viewModel.FormStatus = Convert.ToBoolean(
                    HttpContext.Session.GetString("formStatus"));
                HttpContext.Session.Remove("formStatus");

                // перевіряємо - якщо помилковий, то у сесії дані валідації і моделі
                if (viewModel.FormStatus ?? false)
                {
                    viewModel.FormModel = null;
                    viewModel.FormValidation = null;
                }
                else
                {
                    viewModel.FormModel = JsonSerializer
                        .Deserialize<SignupFormModel>(
                            HttpContext.Session.GetString("formModel")!);
                    HttpContext.Session.Remove("formModel");

                    viewModel.FormValidation = JsonSerializer
                        .Deserialize<SignupFormValidation>(
                            HttpContext.Session.GetString("formValidation")!);
                    HttpContext.Session.Remove("formValidation");
                }
            }

            return View(viewModel);
        }
        [HttpPost]
        public async Task<RedirectToActionResult> SignupForm(SignupFormModel model)
        {
			SignupFormValidation results = new();
            bool isFormValid = true;
            if (String.IsNullOrEmpty(model.Login))
            {
                
                results.LoginErrorMessage = "Логін не може бути порожнім";
                isFormValid = false;
            }
			var existingUser = await LoadUserEntityAsync(model.Login);
			if (existingUser != null)
			{
				results.LoginErrorMessage = "Такий логін вже використовується";
				isFormValid = false;
			}
			if (String.IsNullOrEmpty(model.Name))
            {
                results.NameErrorMessage = "ПІБ не може бути порожнім";
                isFormValid = false;
            }
	

			if (String.IsNullOrEmpty(model.Phone))
            {
                results.PhoneErrorMessage = "Телефон не може бути порожнім";
                isFormValid = false;
            }
            else
            {
				string pattern = @"^\+?\d{12}$|^\+?\d{5}-\d{2}-\d{2}-\d{3}$";
				Regex regex = new Regex(pattern);

				if (!regex.IsMatch(model.Phone))
				{
					results.PhoneErrorMessage = "Формат телефону введено не вірно";
					isFormValid = false;
				}
			}
         
            if (String.IsNullOrEmpty(model.Address))
            {
                results.AddressErrorMessage = "Адреса не може бути порожня";
                isFormValid = false;
            }
            if (String.IsNullOrEmpty(model.Email))
            {
                results.EmailErrorMessage = "Email не може бути порожнім";
                isFormValid = false;
            }
            if (String.IsNullOrEmpty(model.Password))
            {
                results.PasswordErrorMessage = "Пароль не може бути порожнім";
                isFormValid = false;
            }
            if (model.Password != model.Repeat)
            {
                results.RepeatErrorMessage = "Повтор не збігається з паролем";
                isFormValid = false;
            }

            String fileName2= "af713819-7df9-4b78-8752-54408e289352.jpg";
            String fileName = null;

			if (isFormValid && model.Avatar != null &&
                model.Avatar.Length > 0)  //
            {
                
                int dotPosition = model.Avatar.FileName.LastIndexOf(".");
                if (dotPosition == -1)
                {
                    results.AvatarErrorMessage = "Файли без розширення не приймаються";
                    isFormValid = false;
                }
                else
                {
                    String ext = model.Avatar.FileName.Substring(dotPosition);
                    
                    String dir = Directory.GetCurrentDirectory();
                    String savedName;
                    
                    do
                    {
                        fileName = Guid.NewGuid() + ext;
                        savedName = Path.Combine(dir, "wwwroot", "avatars", fileName);
                    }
                    while (System.IO.File.Exists(savedName));
                    using Stream stream = System.IO.File.OpenWrite(savedName);
                    model.Avatar.CopyTo(stream);

                    
                }
            }
          
            
            String salt = _hashService.HexString(Guid.NewGuid().ToString());
            String dk = _hashService.HexString(salt + model.Password);
            if (isFormValid)
            {
                await AddUserAsync(new()
                {
                    Login = model.Login,
                    Name = model.Name,
                    Phone = model.Phone,
                    Address = model.Address,
                    Email = model.Email,
                    PasswordSalt = salt,
                    PasswordDk = dk,
                    Avatar = fileName

                });
            }

            if (!isFormValid)
            {
                HttpContext.Session.SetString("formModel",
                    JsonSerializer.Serialize(model));

                HttpContext.Session.SetString("formValidation",
                    JsonSerializer.Serialize(results));
            }
            HttpContext.Session.SetString("formStatus",
                isFormValid.ToString());

            return RedirectToAction(nameof(Sіgnup));
        }

        private async Task<UserEntity> LoadUserEntityAsync(String userLogin)
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
                    break; 
                }
            }

            if (userEntity == null)
            {
                Console.WriteLine("User not found.");
            }

            return userEntity;
        }
        private async Task<UserEntity> LoadUserByIdAsync(String sid)
        {
			
			// Шукаємо у контексті даних (у БД)
			CosmosClient cosmosClient = _dbService.GetCosmosClient();
			string dbName = _configuration.GetSection("CosmosDB").GetSection("DbName").Value;
			Database database = await cosmosClient.CreateDatabaseIfNotExistsAsync(dbName);
			Container container = await database.CreateContainerIfNotExistsAsync("Items", "/partitionKey");
			string userId = sid; // Замените на фактический ID пользователя
			string partitionKey = "User"; // Замените на фактическое значение partition key      

			ItemResponse<UserEntity> response = await container.ReadItemAsync<UserEntity>(userId, new PartitionKey(partitionKey));
			return response.Resource;
        }

        [HttpPost]
        public async Task<JsonResult> UpdateProfile(String newName, String newEmail)
        {
            // перевіряємо автентифікацію
            if (HttpContext.User.Identity?.IsAuthenticated ?? false)
            {
                // шукаємо дані користувача за Claim
                String sid = HttpContext
                    .User
                    .Claims
                    .First(claim => claim.Type == ClaimTypes.Sid)
                    .Value;
                // Шукаємо у контексті даних (у БД)
                UserEntity user = await LoadUserByIdAsync(sid);
                if (user != null)
                {
                    // Вносимо зміни
                    user.Name = newName;
                    user.Email = newEmail;
                    CosmosClient cosmosClient = _dbService.GetCosmosClient();
                    string dbName = _configuration.GetSection("CosmosDB").GetSection("DbName").Value;
                    Database database = await cosmosClient.CreateDatabaseIfNotExistsAsync(dbName);
                    Container container = await database.CreateContainerIfNotExistsAsync("Items", "/partitionKey");

                    // Обновляем элемент в базе данных
                    ItemResponse<UserEntity> updateResponse = await container.ReplaceItemAsync(user, user.Id, new PartitionKey("User"));
                    return Json(new { status = 200 });
                }
            }
            // сюди потрапляємо або якщо не автентифікований, або не знайдений
            return Json(new { status = 40555 });
        }

        private async Task AddUserAsync(UserEntity user)
        {
            CosmosClient cosmosClient = _dbService.GetCosmosClient();


            String dbName = _configuration.GetSection("CosmosDB").GetSection("DbName").Value;
            Database database = await cosmosClient
             .CreateDatabaseIfNotExistsAsync(dbName);

            Microsoft.Azure.Cosmos.Container container = await database
                .CreateContainerIfNotExistsAsync("Items", "/partitionKey");

            ItemResponse<UserEntity> userResponse =
              await container.CreateItemAsync<UserEntity>(
              user,
              new PartitionKey(user.PartitionKey)
            );
        }



        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
