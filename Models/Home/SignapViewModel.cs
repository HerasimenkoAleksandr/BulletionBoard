using BulletionBoard.Data.Users;

namespace BulletionBoard.Models.Home
{
    public class SignupViewModel
    {
        public SignupFormModel? FormModel { get; set; }
        public SignupFormValidation? FormValidation { get; set; }

        public List<UserEntity>? userEntities { get; set; }
        public bool? FormStatus { get; set; }

    }
}
