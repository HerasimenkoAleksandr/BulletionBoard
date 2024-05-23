using Microsoft.AspNetCore.Mvc;
using System.Text.Json.Serialization;

namespace BulletionBoard.Models.Ads
{ 
    public class AdsFormModel
    {
        [FromForm(Name = "ads-theme")]
        public String Theme { get; set; } = null!;

        [FromForm(Name = "ads-name")]
        public String Name { get; set; } = null!;

        [FromForm(Name = "ads-description")]
        public String Description { get; set; } = null!;

        [FromForm(Name = "ads-file")]
        [JsonIgnore]
        public IFormFile Picture { get; set; } = null!;
    }
}
