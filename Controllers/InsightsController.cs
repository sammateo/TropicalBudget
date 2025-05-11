using Microsoft.AspNetCore.Mvc;
using TropicalBudget.Models;
using TropicalBudget.Services;
using TropicalBudget.Utilities;

namespace TropicalBudget.Controllers
{
    public class InsightsController : Controller
    {
        private readonly DatabaseService _db;
        public InsightsController(DatabaseService db)
        {
            _db = db;
        }
        public async Task<IActionResult> Index(Guid budgetID, int? year, int? month)
        {
            if (budgetID == Guid.Empty)
                return RedirectToAction("Index", "Home");
            Tuple<Guid, List<Transaction>> budgetTransactions = new(new(), new());
            try
            {
                string userID = UserUtility.GetUserID(User);
                DateTime currentDate = DateTime.Now;
                string currentMonth = string.Empty;
                DateTime startDate;
                DateTime endDate;
                if (year == null || month == null)
                {
                    currentMonth = $"{currentDate.ToString("MMMM")}, {currentDate.ToString("yyyy")}";
                    //get start and end date of the month
                    startDate = new DateTime(currentDate.Year, currentDate.Month, 1, 0, 0, 0);
                    endDate = startDate.AddMonths(1).AddSeconds(-1);
                }
                else
                {
                    if (month.Value > 12 || month.Value < 1)
                    {
                        return RedirectToAction("Index");
                    }
                    startDate = new DateTime(year.Value, month.Value, 1, 0, 0, 0);
                    endDate = startDate.AddMonths(1).AddSeconds(-1);
                    currentMonth = $"{startDate.ToString("MMMM")}, {startDate.ToString("yyyy")}";
                }
                TempData["currentMonthString"] = currentMonth;
                TempData["startDate"] = startDate;
                Budget budget = await _db.GetBudget(userID, budgetID);
                TempData["BudgetName"] = budget != null && !string.IsNullOrWhiteSpace(budget.Name) ? budget.Name : "Unknown";
                List<Transaction> transactions = await _db.GetTransactions(budgetID, startDate, endDate);
                budgetTransactions = new(budgetID, transactions);
            }
            catch (Exception ex)
            {
                SentrySdk.CaptureException(ex);
            }
            return View("ViewInsights", budgetTransactions);
        }
    }
}
