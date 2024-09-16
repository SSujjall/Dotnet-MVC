using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using MVC.Models.Entities;
using Newtonsoft.Json;
using System.Security.Claims;
using System.Text;

namespace MVC.Controllers
{
    public class GoogleSignupController : Controller
    {
        private readonly string baseUrl = "https://localhost:7198";

        public IActionResult Login()
        {
            var token = HttpContext.Session.GetString("JWToken");
            if (!string.IsNullOrEmpty(token))
            {
                TempData["Message"] = "Login successful!";
                return RedirectToAction("GetUserDetail", "BearerAuth"); // Redirect if logged in
            }

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> UserLogin(UserLogin userLogin)
        {
            using (var client = new HttpClient())
            {
                var json = JsonConvert.SerializeObject(userLogin);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PostAsync($"{baseUrl}/User/Login", content);

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadAsStringAsync();
                    var tokenResponse = JsonConvert.DeserializeObject<TokenResponse>(result);

                    // Store the token in session
                    HttpContext.Session.SetString("JWToken", tokenResponse.data.token);

                    TempData["Message"] = "Login successful!";
                    //return RedirectToAction("GetAllList");
                    return RedirectToAction("GetStudents", "Student");
                    //return RedirectToAction("GetUserDetail");
                }
                else
                {
                    TempData["Message"] = "Login failed!";
                    return RedirectToAction("Login");
                }
            }
        }

        [HttpGet]
        public IActionResult GoogleLogin()
        {
            var redirectUrl = Url.Action("GoogleResponse", "GoogleSignup");
            var properties = new AuthenticationProperties { RedirectUri = redirectUrl };
            return Challenge(properties, GoogleDefaults.AuthenticationScheme);
        }

        public async Task<IActionResult> GoogleResponse(string password)
        {
            var result = await HttpContext.AuthenticateAsync("Google");

            if (result?.Principal == null)
                return RedirectToAction(nameof(Login));

            var claims = result.Principal.Identities
                            .FirstOrDefault().Claims
                            .Select(claim => new
                            {
                                claim.Issuer,
                                claim.OriginalIssuer,
                                claim.Type,
                                claim.Value
                            });

            var email = claims.FirstOrDefault(x => x.Type == ClaimTypes.Email)?.Value.ToLower();

            if (email == null)
                return RedirectToAction(nameof(Login));

            var userLogin = new UserLogin
            {
                Username = email
            };

            // Store the email in TempData and redirect to AskPassword
            HttpContext.Session.SetString("GoogleUser", JsonConvert.SerializeObject(userLogin));

            return RedirectToAction("AskPassword");
        }


        public IActionResult AskPassword()
        {
            var googleUserJson = HttpContext.Session.GetString("GoogleUser");
            var googleUser = JsonConvert.DeserializeObject<UserLogin>(googleUserJson);

            if (googleUser == null)
                return RedirectToAction(nameof(Login));

            ViewBag.GoogleUser = googleUser;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AskPassword(UserLogin userLogin)
        {
            var googleUserJson = HttpContext.Session.GetString("GoogleUser");
            var googleUser = JsonConvert.DeserializeObject<UserLogin>(googleUserJson);

            if (googleUser == null)
                return RedirectToAction(nameof(Login));

            userLogin.Username = googleUser.Username;
            var token = await RegisterOrLoginUser(userLogin);

            if (!string.IsNullOrEmpty(token))
            {
                HttpContext.Session.SetString("JWToken", token);
                return RedirectToAction("GetStudents", "Student");
            }
            else
            {
                TempData["Message"] = "Login failed!";
                return RedirectToAction("Login");
            }
        }


        private async Task<string> RegisterOrLoginUser(UserLogin userLogin)
        {
            using (var client = new HttpClient())
            {
                // Register User if not registered
                var content = new StringContent(JsonConvert.SerializeObject(userLogin), Encoding.UTF8, "application/json");
                var registerResponse = await client.PostAsync($"{baseUrl}/User/Register", content);

                // Checking the response contents
                var registerResult = await registerResponse.Content.ReadAsStringAsync();

                var loginResponse = await client.PostAsync($"{baseUrl}/User/Login", new StringContent(JsonConvert.SerializeObject(userLogin), Encoding.UTF8, "application/json"));
                if (loginResponse.IsSuccessStatusCode)
                {
                    var result = await loginResponse.Content.ReadAsStringAsync();
                    var tokenResponse = JsonConvert.DeserializeObject<TokenResponse>(result);
                    return tokenResponse.data.token;
                }
            }
            return null;
        }
    }
}
