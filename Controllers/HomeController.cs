using System.Diagnostics;
using System.Security.Claims;
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

        public async Task<IActionResult> Index(int? year, int? month)
        {
            string userID = UserUtility.GetUserID(User);
            List<Budget> transactions = await _db.GetBudgets(userID);
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
