namespace BulletionBoard.Services.Hash
{
    public class Md5HashService : IHashService
    {
        public string HexString(string input)
        {
            using var hasher = System.Security.Cryptography.MD5.Create();
            byte[] bytes = hasher.ComputeHash(
                System.Text.Encoding.UTF8.GetBytes(input)
            );
            return Convert.ToHexString(bytes);
        }
    }
}
