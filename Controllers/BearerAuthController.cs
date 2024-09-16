using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Facebook;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using MVC.Models.DTO;
using MVC.Models.Entities;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;

namespace MVC.Controllers
{
    public class BearerAuthController : Controller
    {
        private readonly string baseUrl = "https://localhost:7198";

        public IActionResult Login()
        {
            var token = HttpContext.Session.GetString("JWToken");
            if (!string.IsNullOrEmpty(token))
            {
                TempData["Message"] = "Login successful!";
                return RedirectToAction("GetUserDetail"); // Redirect if logged in
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


        //// This was for the GetAllList method in the ListController of the ToDo Web API (not for the other Web API)
        //[HttpGet]
        //public async Task<IActionResult> GetAllList()
        //{
        //    var token = HttpContext.Session.GetString("JWToken");

        //    if (string.IsNullOrEmpty(token))
        //    {
        //        TempData["Message"] = "You need to log in first.";
        //        return RedirectToAction("Index");
        //    }

        //    using (var client = new HttpClient())
        //    {
        //        // Set the Bearer token in the request header
        //        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        //        var response = await client.GetAsync($"{baseUrl}/List/GetAllList");

        //        if (response.IsSuccessStatusCode)
        //        {
        //            var result = await response.Content.ReadAsStringAsync();
        //            var listResponse = JsonConvert.DeserializeObject<ListResponse>(result);

        //            return View(listResponse.data); // Pass the list of ListData to the view
        //        }
        //        else
        //        {
        //            TempData["Message"] = "Failed to retrieve the list!";
        //            return RedirectToAction("Login");
        //        }
        //    }
        //}

        [HttpGet]
        public IActionResult FacebookLogin()
        {
            var redirectUrl = Url.Action("FacebookResponse", "BearerAuth");
            var properties = new AuthenticationProperties { RedirectUri = redirectUrl };
            return Challenge(properties, FacebookDefaults.AuthenticationScheme);
        }

        public async Task<IActionResult> FacebookResponse()
        {
            var result = await HttpContext.AuthenticateAsync("Facebook");

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

            var nameIdentifier = claims.Where(x => x.Type == ClaimTypes.NameIdentifier).FirstOrDefault()?.Value;
            var rawName = claims.FirstOrDefault(x => x.Type == ClaimTypes.Name)?.Value.ToLower();
            var filteredName = rawName?.Replace(" ", "");


            var userLogin = new UserLogin
            {
                Username = $"{filteredName}.{nameIdentifier}",
                Password = claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value // send the unique name idenfier from the claims as password.
            };

            var token = await RegisterOrLoginUser(userLogin);

            if (!string.IsNullOrEmpty(token))
            {
                HttpContext.Session.SetString("JWToken", token);
                return RedirectToAction("GetUserDetail");
            }
            else
            {
                TempData["Message"] = "Login failed!";
                return RedirectToAction("Login");
            }
        }


        [HttpGet]
        public IActionResult GoogleLogin()
        {
            var redirectUrl = Url.Action("GoogleResponse", "BearerAuth");
            var properties = new AuthenticationProperties { RedirectUri = redirectUrl };
            return Challenge(properties, GoogleDefaults.AuthenticationScheme);
        }

        public async Task<IActionResult> GoogleResponse()
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

            var userLogin = new UserLogin
            {
                Username = claims.FirstOrDefault(x => x.Type == ClaimTypes.Email)?.Value.ToLower(),
                Password = claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value // send the unique name idenfier from the claims as password.
            };

            var token = await RegisterOrLoginUser(userLogin);

            if (!string.IsNullOrEmpty(token))
            {
                HttpContext.Session.SetString("JWToken", token);
                return RedirectToAction("GetUserDetail");
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

        public IActionResult GetUserDetail()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetUserDetailApi()
        {
            var token = HttpContext.Session.GetString("JWToken");

            if (string.IsNullOrEmpty(token))
            {
                TempData["Message"] = "You need to log in first.";
                return RedirectToAction("Index");
            }

            // Decode the JWT token to extract the UserId (claims bata haina direct token string ma liyera decode gareko)
            var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);
            var userIdClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "UserId");

            if (userIdClaim == null)
            {
                return Json(new { success = false, message = "Failed to retrieve user information from token!" });
            }

            var id = userIdClaim.Value;

            using (var client = new HttpClient())
            {
                // Set the Bearer token in the request header
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var response = await client.GetAsync($"{baseUrl}/User/GetUserInfo/{id}");

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadAsStringAsync();
                    var res = JsonConvert.DeserializeObject<UserResponse>(result);

                    return Json(new { success = true, username = res?.Data?.Username });
                }
                else
                {
                    return Json(new { success = false, message = "Failed to retrieve user details!" });
                }
            }
        }

        public async Task<IActionResult> Logout()
        {
            HttpContext.Session.Remove("JWToken");
            HttpContext.Session.Remove("Username");
            Response.Cookies.Delete("JWToken");
            TempData["Message"] = "Logged out successfully!";
            return RedirectToAction("Login");
        }
    }
}
