using BulletionBoard.Data.Users;
using BulletionBoard.Data.Ads;
using BulletionBoard.Models.PicturesHome;


namespace BulletionBoard.Models.Ads
{
    public class AdsViewModel
    {
        public AdsFormModel? FormModel { get; set; }
        public AdsFormValidation? FormValidation { get; set; }

        public PicturesFindModel Page { get; set; }

        public IndexViewModel Theme { get; set; } = new ();
        
        public List<AdsEntity>? AdsEntities { get; set; }

        public List<UserEntity>? userEntities { get; set; }

        public bool? FormStatus { get; set; }

    }
}
