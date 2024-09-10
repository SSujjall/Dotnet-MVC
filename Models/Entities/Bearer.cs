using System.Data;

namespace MVC.Models.Entities
{
    public class UserLogin
    {
        public string Username { get; set; }
        public string Password { get; set; } 
    }

    //For the ToDo Web API
    //public class TokenResponse
    //{
    //    public int statusCode { get; set; }
    //    public TokenData data { get; set; }
    //    public List<string> errors { get; set; }
    //}

    //public class TokenData
    //{
    //    public string token { get; set; }
    //    public DateTime expiration { get; set; }
    //    public string userId { get; set; }
    //}

    public class TokenResponse
    {
        public int StatusCode { get; set; }
        public TokenData data { get; set; }
    }

    public class TokenData
    {
        public string token { get; set; }
    }

    public class UserRegister
    {
        public string userName { get; set; }
        public string firstName { get; set; }
        public string lastName { get; set; }
        public string email { get; set; }
        public string password { get; set; }
    }

    public class RegisterResponse
    {
        public int statusCode { get; set; }
        public string data { get; set; }
        public List<string> errors { get; set; }
    }


    public class ListResponse
    {
        public int statusCode { get; set; }
        public List<ListData> data { get; set; }
        public List<string> errors { get; set; }
    }

    public class ListData
    {
        public int id { get; set; }
        public string listName { get; set; }
        public string userId { get; set; }
        public DateTime createdAt { get; set; }
    }

    public class UserResponse
    {
        public Datas Data { get; set; }
        public int StatusCode { get; set; }
    }

    public class Datas
    {
        public string Username { get; set; }
    }
}