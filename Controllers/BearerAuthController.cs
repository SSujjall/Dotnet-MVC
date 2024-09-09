using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using MVC.Models.DTO;
using MVC.Models.Entities;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Text;

namespace MVC.Controllers
{
    public class BearerAuthController : Controller
    {
        private readonly string baseUrl = "https://localhost:7077/api";

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> UserLogin(UserLogin userLogin)
        {
            using (var client = new HttpClient())
            {
                var json = JsonConvert.SerializeObject(userLogin);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PostAsync($"{baseUrl}/Auth/Login", content);

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadAsStringAsync();
                    var tokenResponse = JsonConvert.DeserializeObject<TokenResponse>(result);

                    // Store the token in session
                    HttpContext.Session.SetString("JWToken", tokenResponse.data.token);

                    TempData["Message"] = "Login successful!";
                    return RedirectToAction("GetAllList");
                }
                else
                {
                    TempData["Message"] = "Login failed!";
                    return RedirectToAction("Login");
                }
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAllList()
        {
            var token = HttpContext.Session.GetString("JWToken");

            if (string.IsNullOrEmpty(token))
            {
                TempData["Message"] = "You need to log in first.";
                return RedirectToAction("Index");
            }

            using (var client = new HttpClient())
            {
                // Set the Bearer token in the request header
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var response = await client.GetAsync($"{baseUrl}/List/GetAllList");

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadAsStringAsync();
                    var listResponse = JsonConvert.DeserializeObject<ListResponse>(result);

                    return View(listResponse.data); // Pass the list of ListData to the view
                }
                else
                {
                    TempData["Message"] = "Failed to retrieve the list!";
                    return RedirectToAction("Login");
                }
            }
        }

        public async Task<IActionResult> Logout()
        {
            HttpContext.Session.Remove("JWToken");
            TempData["Message"] = "Logged out successfully!";
            return RedirectToAction("Login");
        }
    }
}
