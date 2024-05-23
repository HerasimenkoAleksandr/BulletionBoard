using Newtonsoft.Json;

namespace BulletionBoard.Data.Users
{
    public class UserEntity
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [JsonProperty(PropertyName = "partitionKey")]
        public string PartitionKey { get; set; } = "User";
        public String Login { get; set; } = null!;
        public String? Name { get; set; }
        public String? Phone { get; set; }
        public String? Address { get; set; }
        public String Email { get; set; } = null!;
        public String PasswordSalt { get; set; } = null!;
        public String PasswordDk { get; set; } = null!;
        public String Avatar { get; set; } = null!;
        public DateTime RegisterDt { get; set; } = DateTime.Now;
        public DateTime? DeleteDt { get; set; }
    }
}
