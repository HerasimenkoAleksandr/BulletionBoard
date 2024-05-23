using Microsoft.AspNetCore.Mvc;
using System.Text.Json.Serialization;

namespace BulletionBoard.Models.Home
{
    public class SignupFormModel
    {
        [FromForm(Name = "signup-login")]
        public String Login { get; set; } = null!;

        [FromForm(Name = "signup-name")]
        public String Name { get; set; } = null!;

		[FromForm(Name = "signup-phone")]
		public String Phone { get; set; } = null!;
        
        [FromForm(Name = "signup-address")]
        public String Address { get; set; } = null!;

        [FromForm(Name = "signup-email")]
        public String Email { get; set; } = null!;

        [FromForm(Name = "signup-password")]
        public String Password { get; set; } = null!;

        [FromForm(Name = "signup-repeat")]
        public String Repeat { get; set; } = null!;

        [FromForm(Name = "signup-avatar")]
        [JsonIgnore]
        public IFormFile Avatar { get; set; } = null!;
    }
}
