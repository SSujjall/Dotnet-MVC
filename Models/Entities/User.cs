namespace MVC.Models.Entities
{
    public class User
    {
        public int UserId { get; set; }
        public string Username { get; set; }
        public string Role { get; set; }
        public string PasswordHash { get; set; }
    }
}
