using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TropicalBudget.Models;
using TropicalBudget.Models.ViewModels;
using TropicalBudget.Services;
using TropicalBudget.Utilities;

namespace TropicalBudget.Controllers
{
    // [Route("api/[controller]")]
    // [ApiController]
    [Authorize]
    public class SavingsGoalsController : Controller
    {
        private readonly DatabaseService _db;
        public SavingsGoalsController(DatabaseService db)
        {
            _db = db;
        }

        public async Task<IActionResult> Index(Guid budgetID, int? year, int? month)
        {
            if (budgetID == Guid.Empty)
                return RedirectToAction("Index", "Home");

            ViewSavingsGoalsViewModel viewSavingsGoalsViewModel = new();
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
                TempData["startDate"] = startDate;
                Budget budget = await _db.GetBudget(userID, budgetID);
                TempData["BudgetName"] = budget != null && !string.IsNullOrWhiteSpace(budget.Name) ? budget.Name : "Unknown";
                List<SavingsGoal> savingsGoals = await _db.GetSavingsGoals(budgetID);
                List<Transaction> transactions = await _db.GetTransactions(budgetID);
                viewSavingsGoalsViewModel = new()
                {
                    Budget = budget ?? new(),
                    StartDate = startDate,
                    SavingsGoals = savingsGoals,
                    Transactions = transactions
                };
            }
            catch (Exception ex)
            {
                SentrySdk.CaptureException(ex);
            }

            return View("ViewSavingsGoals", viewSavingsGoalsViewModel);
        }

        public async Task<IActionResult> New(Guid budgetID, int? year, int? month)
        {
            if (budgetID == Guid.Empty || year == null || month == null)
                return RedirectToAction("Index", "Home");

            SavingsGoal newSavingsGoal = new();
            try
            {
                string userID = UserUtility.GetUserID(User);
                newSavingsGoal.BudgetID = budgetID;
                newSavingsGoal.Month = month.Value;
                newSavingsGoal.Year = year.Value;
            }
            catch (Exception ex)
            {
                SentrySdk.CaptureException(ex);
            }

            return View("NewSavingsGoal", newSavingsGoal);
        }

        public async Task<IActionResult> Edit(Guid budgetID, Guid savingsGoalID, int? year, int? month)
        {
            if (budgetID == Guid.Empty || savingsGoalID == Guid.Empty || year == null || month == null)
                return RedirectToAction("Index", "Home");

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
            SavingsGoal editPlanItem = new();
            try
            {
                string userID = UserUtility.GetUserID(User);
                editPlanItem = await _db.GetSavingsGoal(savingsGoalID, userID);
                editPlanItem.Year = startDate.Year;
                editPlanItem.Month = startDate.Month;
                List<Transaction> transactions = await _db.GetTransactions(budgetID);

            }
            catch (Exception ex)
            {
                SentrySdk.CaptureException(ex);
            }
            return View("EditSavingsGoal", editPlanItem);
        }



        public async Task<IActionResult> AddNewSavingsGoal(SavingsGoal savingsGoal)
        {
            try
            {
                await _db.InsertSavingsGoal(savingsGoal);
            }
            catch (Exception ex)
            {
                SentrySdk.CaptureException(ex);
            }
            return RedirectToAction("Index", new { budgetID = savingsGoal.BudgetID, year = savingsGoal.Year, month = savingsGoal.Month });
        }

        public async Task<IActionResult> EditSavingsGoal(SavingsGoal savingsGoal)
        {
            try
            {
                await _db.UpdateSavingsGoal(savingsGoal);
            }
            catch (Exception ex)
            {
                SentrySdk.CaptureException(ex);
            }
            return RedirectToAction("Index", new { budgetID = savingsGoal.BudgetID, year = savingsGoal.Year, month = savingsGoal.Month });
        }

        public async Task<IActionResult> DeleteSavingsGoal(Guid budgetID, Guid savingsGoalID, int? year, int? month)
        {
            if (budgetID == Guid.Empty || savingsGoalID == Guid.Empty || year == null || month == null)
                return RedirectToAction("Index", "Home");
            try
            {
                await _db.DeleteSavingsGoal(savingsGoalID);
            }
            catch (Exception ex)
            {
                SentrySdk.CaptureException(ex);
            }
            return RedirectToAction("Index", new { budgetID, year, month });
        }

    }
}
