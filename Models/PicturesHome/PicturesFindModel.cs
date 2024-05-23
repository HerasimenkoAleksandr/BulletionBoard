using Microsoft.AspNetCore.Mvc;
using System.Text.Json.Serialization;

namespace BulletionBoard.Models.PicturesHome
{
    public class PicturesFindModel
    {  
        public String? PathToPictures { get; set; }     
        public String? Name { get; set; }
        public String? NameForTitle { get; set; }
        public String? Id { get; set; }

    }
}
