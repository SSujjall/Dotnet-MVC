using System.Runtime.InteropServices.JavaScript;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NewWeb.Data;
using NewWeb.Models.Entities;
using Newtonsoft.Json;
using MVC.Models.DTO;

namespace MVC.Controllers
{
    public class CheckoutController : Controller
    {
        private readonly AppDbContext _dbContext;
        private readonly string _addCheckoutUrl = "https://localhost:7198/Checkout/AddCheckoutWeb";
        private readonly string _updateValueUrl = "https://localhost:7198/Checkout/ResponseCheckout";
        private readonly string _softDeleteUrl = "https://localhost:7198/Checkout/SoftDeleteCheckout";

        public CheckoutController(AppDbContext dbContext)
        {
            this._dbContext = dbContext;
        }


        [HttpGet]
        public async Task<IActionResult> GetCheckout(string sortByName = null, string sortByAmount = null, string sortByStatus = null)
        {
            var checkouts = _dbContext.Checkouts.AsQueryable();

            if (!string.IsNullOrEmpty(sortByName))
            {
                checkouts = sortByName == "asc" ? checkouts.OrderBy(c => c.Name) : checkouts.OrderByDescending(c => c.Name);
            }

            if (!string.IsNullOrEmpty(sortByAmount))
            {
                checkouts = sortByAmount == "asc" ? checkouts.OrderBy(c => c.Amount) : checkouts.OrderByDescending(c => c.Amount);
            }

            if (!string.IsNullOrEmpty(sortByStatus))
            {
                sortByStatus = sortByStatus.ToUpper();
                checkouts = checkouts.Where(c => c.Status.ToUpper() == sortByStatus);
            }

            var studentView = await checkouts.ToListAsync();

            return View(studentView);
        }


        [HttpGet]
        public IActionResult AddCheckout()
        {
            return View();
        }


        [HttpPost]
        public async Task<IActionResult> AddCheckout(CheckoutDTO checkoutModel)
        {
            Random random = new Random();
            int num = random.Next(1, 7);


            var check = new Checkout()
            {
                Name = checkoutModel.Name,
                Amount = checkoutModel.Amount,
                Remarks = checkoutModel.Remarks,
            };

            using (var httpClient = new HttpClient())
            {
                var jsonContent = JsonConvert.SerializeObject(checkoutModel);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                using (var httpResponse = await httpClient.PostAsync(_addCheckoutUrl, content))
                {
                    var responseContent = await httpResponse.Content.ReadAsStringAsync();
                    var apiResponse = JsonConvert.DeserializeObject<Checkout>(responseContent);

                    if (num <= 3)
                    {
                        check.Status = apiResponse.Status;
                        check.Id = apiResponse.Id;
                    }
                    else
                    {
                        check.Status = "Pending";
                        check.Id = apiResponse.Id;
                    }

                    await _dbContext.Checkouts.AddAsync(check);
                    await _dbContext.SaveChangesAsync();
                    return RedirectToAction("GetCheckout");
                }
            }
        }


        [HttpPost]
        public async Task<IActionResult> DeleteCheckout(Guid id)
        {
            var eachData = await _dbContext.Checkouts.FindAsync(id);

            if (eachData != null)
            {
                using (var httpClient = new HttpClient())
                {
                    var response = await httpClient.PutAsync($"{_softDeleteUrl}/{id}", null);
                }

                _dbContext.Checkouts.Remove(eachData);
                await _dbContext.SaveChangesAsync();
            }

            return RedirectToAction("GetCheckout", "Checkout");
        }

        [HttpPost]
        public async Task<IActionResult> UpdateCheckout(Guid id)
        {
            var checkout = await _dbContext.Checkouts.FindAsync(id);
            if (checkout == null)
            {
                return NotFound();
            }

            using (var httpClient = new HttpClient())
            {
                var response = await httpClient.GetStringAsync($"{_updateValueUrl}/{id}");
                var apiResponse = JsonConvert.DeserializeObject<Checkout>(response);

                checkout.Status = apiResponse.Status;
                await _dbContext.SaveChangesAsync();
            }

            return RedirectToAction("GetCheckout");
        }
    }
}