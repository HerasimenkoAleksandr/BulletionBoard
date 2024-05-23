namespace BulletionBoard.Models.Home
{
	public class UserProfileViewModel
	{
		public bool IsPersonal { get; set; }
		public Data.Users.UserEntity? User { get; set; }
	}
}
