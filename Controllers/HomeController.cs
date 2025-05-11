using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TropicalBudget.Models;
using TropicalBudget.Services;
using TropicalBudget.Utilities;

namespace TropicalBudget.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly DatabaseService _db;

        public HomeController(ILogger<HomeController> logger, DatabaseService db)
        {
            _logger = logger;
            _db = db;
        }

        public async Task<IActionResult> Index()
        {
            List<Budget> transactions = new();
            try
            {
                string userID = UserUtility.GetUserID(User);
                transactions = await _db.GetBudgets(userID);
            }
            catch (Exception ex)
            {
                SentrySdk.CaptureException(ex);
            }
            return View(transactions);
        }

        public async Task<IActionResult> NewBudget()
        {
            Budget newBudget = new();
            return View(newBudget);
        }

        public async Task<IActionResult> AddNewBudget(Budget newBudget)
        {
            string userID = UserUtility.GetUserID(User);
            newBudget.UserID = userID;
            await _db.InsertBudget(newBudget);
            return RedirectToAction("Index");
        }
        [HttpPost]
        public async Task<IActionResult> UpdateBudgetName(Guid budgetID, string name)
        {
            try
            {
                string userID = UserUtility.GetUserID(User);
                Budget newBudget = new()
                {
                    Name = name,
                    ID = budgetID,
                    UserID = userID
                };
                await _db.UpdateBudget(newBudget);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return BadRequest($"An error occurred while processing your request");
            }
                return Ok("success");
            
        }

       
        [HttpDelete]
        public async Task<IActionResult> DeleteBudget(Guid budgetID)
        {
            try
            {
                string userID = UserUtility.GetUserID(User);
                Budget deletedBudget = new()
                {
                    ID = budgetID,
                    UserID = userID
                };
                await _db.DeleteBudget(deletedBudget);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return BadRequest($"An error occurred while processing your request");
            }
            return RedirectToAction("Index");
        }







        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
