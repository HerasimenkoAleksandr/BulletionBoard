using BulletionBoard.Models.Ads;
using BulletionBoard.Data.Ads;
using BulletionBoard.Models.PicturesHome;
using Microsoft.AspNetCore.Mvc;
using BulletionBoard.Services.Hash;
using System.Text.Json;
using Microsoft.Azure.Cosmos;
using BulletionBoard.Services.ServiceDependencies;
using BulletionBoard.Data.Users;
using System.Security.Claims;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Mvc.Razor.Infrastructure;
using BulletionBoard.Models.Home;
using System.Collections.Generic;


namespace BulletionBoard.Controllers
{
	public class AdsController : Controller
	{
        private readonly ILogger<HomeController> _logger;
        private readonly IConfiguration _configuration;
        private readonly IDbService _dbService;
        
        public AdsController(ILogger<HomeController> logger, IConfiguration configuration, IDbService dbService, IHashService hashService)
        {
            _logger = logger;
            _configuration = configuration;
            _dbService = dbService;
        }

        public async Task <IActionResult> AdsAsync(String id)
		{
			AdsViewModel model = new();
			IndexViewModel indexViewModel = new();
			if (id != null && model is not null)
			{
				for (int i = 0; i < indexViewModel.PicturesModels.Count; i++)
				{

					if (indexViewModel.PicturesModels[i].Id.Equals(id))
					{
                        model.Page = indexViewModel.PicturesModels[i];
                        String? theme = model.Page.Name;
                        if (theme != null)
                        { 
                            model.AdsEntities = await LoadAdsAsync(theme);
                        }
                        break;
					}
				}
                model.userEntities = await LoadUsersAsync();
            }

            return View(model);
		}

        public async Task<List<UserEntity>> LoadUsersAsync()
        {
            List<UserEntity> userEntities = new();
           
            CosmosClient cosmosClient = _dbService.GetCosmosClient();
            string dbName = _configuration.GetSection("CosmosDB").GetSection("DbName").Value;
            Database database = await cosmosClient.CreateDatabaseIfNotExistsAsync(dbName);
            Container container = await database.CreateContainerIfNotExistsAsync("Items", "/partitionKey");

            string partitionKey = "User"; 
            string sqlQueryText = "SELECT * FROM c WHERE c.partitionKey = @partitionKey";

            QueryDefinition queryDefinition = new QueryDefinition(sqlQueryText)
                .WithParameter("@partitionKey", partitionKey);

            FeedIterator<UserEntity> queryResultSetIterator = container.GetItemQueryIterator<UserEntity>(queryDefinition);

            while (queryResultSetIterator.HasMoreResults)
            {
                FeedResponse<UserEntity> currentResultSet = await queryResultSetIterator.ReadNextAsync();
                foreach (UserEntity entity in currentResultSet)
                {
                    userEntities.Add(entity);
                }
            }

            return userEntities;
        }


        public Task<IActionResult> AdsForm()
		{
            AdsViewModel viewModel = new();

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
                        .Deserialize<AdsFormModel>(
                            HttpContext.Session.GetString("formModel")!);
                    HttpContext.Session.Remove("formModel");

                    viewModel.FormValidation = JsonSerializer
                        .Deserialize<AdsFormValidation>(
                            HttpContext.Session.GetString("formValidation")!);
                    HttpContext.Session.Remove("formValidation");
                }
            }

            return Task.FromResult<IActionResult>(View(viewModel));
        }

        
        [HttpPost]
        public async Task<RedirectToActionResult> AdsFromForm(AdsFormModel model)
		{
            AdsFormValidation results = new();
            bool isFormValid = true;
            if (String.IsNullOrEmpty(model.Name))
            {
                results.NameErrorMessage = "Поле назви(теми) не може бути порожнім";
                isFormValid = false;
            }
            if (String.IsNullOrEmpty(model.Description))
            {
                results.DescriptionErrorMessage = "Оголошення не може бути порожнім";
                isFormValid = false;
            }

            String fileName = "depositphotos_12629953-stock-photo-3d-man-holding-blank-board.jpg";
            String relativePath;
            if (isFormValid && model.Picture != null &&
                model.Picture.Length > 0)  // поле не обов'язкове, але якщо є, то перевіряємо
            {
                // при збереженні (uploading) файлів слід міняти їх імена.
                int dotPosition = model.Picture.FileName.LastIndexOf(".");
                if (dotPosition == -1)
                {
                    results.PictureErrorMessage = "Файли без розширення не приймаються";
                    isFormValid = false;
                }
                else
                {
                    String ext = model.Picture.FileName.Substring(dotPosition);
                    // TODO: додати перевірку розширення на перелік дозволених

                    // генеруємо випадкове ім'я файлу, зберігаємо розширення
                    // контролюємо, що такого імені немає у сховищі
                    String dir = Directory.GetCurrentDirectory();
                    String savedName;
                   

                    do
                    {
                        fileName = Guid.NewGuid() + ext;
                        savedName = Path.Combine(dir, "wwwroot", "picturesAds", fileName);
                      
                    }
                    while (System.IO.File.Exists(savedName));
                    using Stream stream = System.IO.File.OpenWrite(savedName);
                    model.Picture.CopyTo(stream);
                }
            }
            relativePath = Path.Combine("picturesAds", fileName);
            if (HttpContext.User.Identity?.IsAuthenticated ?? false)
            {
                String sid = HttpContext
                    .User
                    .Claims
                    .First(claim => claim.Type == ClaimTypes.Sid)
                    .Value;
               if(isFormValid) { 
               await AddAdsAsync(new() {
                UserId=sid,
                Theme = model.Theme,
                Name =model.Name,
                Description = model.Description,
                Pictures = relativePath
                });
                }
            }
            else
            {
                results.DbErrorMessage = "Вам потрібно авторизуватись";
                isFormValid = false;
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

           

            return RedirectToAction(nameof(AdsForm));
		}

        private async Task<List<AdsEntity>> LoadAdsAsync(String theme)
        {
            List<AdsEntity> adsFromTheme = new();
            CosmosClient cosmosClient = _dbService.GetCosmosClient();
            string dbName = _configuration.GetSection("CosmosDB").GetSection("DbName").Value;
            Database database = await cosmosClient.CreateDatabaseIfNotExistsAsync(dbName);
            Container container = await database.CreateContainerIfNotExistsAsync("Items", "/partitionKey");
           
            string partitionKey = "Ads"; // Замените на фактическое значение partition key      

            // Определите запрос для получения элементов, соответствующих теме
            var queryDefinition = new QueryDefinition("SELECT * FROM c WHERE c.Theme = @theme")
                .WithParameter("@theme", theme);

            // Создайте итератор запросов
            using FeedIterator<AdsEntity> queryIterator = container.GetItemQueryIterator<AdsEntity>(queryDefinition, requestOptions: new QueryRequestOptions 
            { PartitionKey = new PartitionKey(partitionKey) });

            // Выполните запрос и добавьте результаты в список
            while (queryIterator.HasMoreResults)
            {
                FeedResponse<AdsEntity> response = await queryIterator.ReadNextAsync();
                adsFromTheme.AddRange(response);
            }

            return adsFromTheme;
        }

        private async Task AddAdsAsync(AdsEntity ads)
        {
            CosmosClient cosmosClient = _dbService.GetCosmosClient();


            String dbName = _configuration.GetSection("CosmosDB").GetSection("DbName").Value;
            Database database = await cosmosClient
             .CreateDatabaseIfNotExistsAsync(dbName);

            Microsoft.Azure.Cosmos.Container container = await database
                .CreateContainerIfNotExistsAsync("Items", "/partitionKey");

            ItemResponse<AdsEntity> userResponse = await container.CreateItemAsync<AdsEntity>(ads, new PartitionKey(ads.PartitionKey));
        }

        [HttpDelete]
        public async Task<JsonResult> DeleteAds(string adsId)
        {
            AdsEntity Ads = await LoadAdsByIdAsync(adsId);
            if (Ads == null) { return Json(new { status = 401 }); }
           
            Ads.DeleteDt = DateTime.Now;  
                                           
            Ads.Theme = "";
            Ads.Name = "";
            Ads.Description = "";
            if (Ads.Pictures != null)
            {  // якщо є - видаляємо файл аватарки
                String dir = Directory.GetCurrentDirectory();
                String avatarFileName = Path.Combine(dir, "wwwroot", "picturesAds", Ads.Pictures);
                if (System.IO.File.Exists(avatarFileName))
                {
                    System.IO.File.Delete(avatarFileName);
                }
                Ads.Pictures = null;
            }

            CosmosClient cosmosClient = _dbService.GetCosmosClient();
            string dbName = _configuration.GetSection("CosmosDB").GetSection("DbName").Value;
            Database database = await cosmosClient.CreateDatabaseIfNotExistsAsync(dbName);
            Container container = await database.CreateContainerIfNotExistsAsync("Items", "/partitionKey");

            // Обновляем элемент в базе данных
            ItemResponse<AdsEntity> updateResponse = await container.ReplaceItemAsync(Ads, Ads.Id, new PartitionKey("Ads"));


            return Json(new { status = 200 });
        }

        private async Task<AdsEntity> LoadAdsByIdAsync(String sid)
        {

           
            CosmosClient cosmosClient = _dbService.GetCosmosClient();
            string dbName = _configuration.GetSection("CosmosDB").GetSection("DbName").Value;
            Database database = await cosmosClient.CreateDatabaseIfNotExistsAsync(dbName);
            Container container = await database.CreateContainerIfNotExistsAsync("Items", "/partitionKey");
            string adsId = sid; 
            string partitionKey = "Ads";   

            ItemResponse<AdsEntity> response = await container.ReadItemAsync<AdsEntity>(adsId, new PartitionKey(partitionKey));
            return response.Resource;
        }



    }
}
