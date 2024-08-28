using Microsoft.AspNetCore.Mvc;
using MVC.Models.Entities;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Text;

namespace MVC.Controllers
{
    public class AuthController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiBaseUrl = "https://localhost:7100/api/Auth";


        public AuthController(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> CallApi()
        {
            string username = "aa";
            string password = "aa";

            AuthResponse authResponse = new AuthResponse();

            var request = new HttpRequestMessage(HttpMethod.Get, $"{_apiBaseUrl}/BasicAuth");

            var credentials = Encoding.UTF8.GetBytes($"{username}:{password}");

            //HEADER eta bata pathaune
            request.Headers.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(credentials));

            using (var response = await _httpClient.SendAsync(request))
            {
                if (response.IsSuccessStatusCode)
                {
                    string apiResponseString = await response.Content.ReadAsStringAsync();
                    authResponse = JsonConvert.DeserializeObject<AuthResponse>(apiResponseString);
                }

                return View("Index", authResponse);
            }
        }
    }
}