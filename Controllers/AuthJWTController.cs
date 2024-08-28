using Microsoft.AspNetCore.Mvc;

namespace MVC.Controllers
{
    public class AuthJWTController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiBaseUrl = "https://localhost:7265/api/Users";

        public AuthJWTController(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> CallApi()
        {
            return null;
        }
    }
}
