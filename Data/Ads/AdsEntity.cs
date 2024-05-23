using Newtonsoft.Json;

namespace BulletionBoard.Data.Ads
{
    public class AdsEntity
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [JsonProperty(PropertyName = "partitionKey")]
        public string PartitionKey { get; set; } = "Ads";
        public String UserId { get; set; } = null!;
        public String Theme { get; set; } = null!;
        public String Name { get; set; } = null!;
        public String Description { get; set; } = null!;
        public String Pictures { get; set; } = null!;
        public DateTime RegisterDt { get; set; } = DateTime.Now;
        public DateTime? DeleteDt { get; set; }
    }
}
