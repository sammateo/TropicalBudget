using Microsoft.AspNetCore.Mvc;
using TropicalBudget.Models;
using TropicalBudget.Services;
using TropicalBudget.Utilities;

namespace TropicalBudget.Controllers
{
    public class PlanController : Controller
    {
        private readonly DatabaseService _db;
        public PlanController(DatabaseService db)
        {
            _db = db;
        }
        public async Task<IActionResult> Index(Guid budgetID, int? year, int? month)
        {
            if (budgetID == Guid.Empty)
                return RedirectToAction("Index", "Home");
            Plan budgetPlan = new();
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

                Budget budget = await _db.GetBudget(userID, budgetID);
                budgetPlan.Budget = budget;
                budgetPlan.startDate = startDate;
                budgetPlan.PlanItems = await _db.GetPlanItems(budgetID);
                budgetPlan.Transactions = await _db.GetTransactions(budgetID, startDate, endDate);
            }
            catch (Exception ex)
            {

                SentrySdk.CaptureException(ex);

            }
            return View("ViewPlan", budgetPlan);
        }

        public async Task<IActionResult> New(Guid budgetID, int? year, int? month)
        {
            if (budgetID == Guid.Empty || year == null || month == null)
                return RedirectToAction("Index", "Home");

            PlanItem newPlanItem = new();
            try
            {
                string userID = UserUtility.GetUserID(User);
                List<PlanItem> planItems = await _db.GetPlanItems(budgetID);
                List<TransactionCategory> transactionCategories = await _db.GetTransactionCategories(userID);
                List<TransactionType> transactionTypes = await _db.GetTransactionTypes();

                //get categories not already in the plan
                transactionCategories = transactionCategories.Where(cat => !planItems.Any(item => item.CategoryID == cat.ID)).ToList();
                transactionCategories = transactionCategories.OrderBy(cat => cat.Name).ToList();
                TempData["TransactionCategories"] = transactionCategories;
                TempData["TransactionTypes"] = transactionTypes;
                newPlanItem.BudgetID = budgetID;
                newPlanItem.Month = month.Value;
                newPlanItem.Year = year.Value;
                if (transactionTypes.Any(type => type.Name.Equals(TransactionUtility.TRANSACTION_TYPE_EXPENSE)))
                {
                    newPlanItem.TransactionTypeID = transactionTypes.FirstOrDefault(type => type.Name.Equals(TransactionUtility.TRANSACTION_TYPE_EXPENSE)).ID;
                }
            }
            catch (Exception ex)
            {
                SentrySdk.CaptureException(ex);
            }

            return View("NewPlanItem", newPlanItem);
        }

        public async Task<IActionResult> Edit(Guid budgetID, Guid planID, int? year, int? month)
        {
            if (budgetID == Guid.Empty || planID == Guid.Empty || year == null || month == null)
                return RedirectToAction("Index", "Home");
            PlanItem editPlanItem = new();
            try
            {
                string userID = UserUtility.GetUserID(User);
                List<PlanItem> planItems = await _db.GetPlanItems(budgetID);
                editPlanItem = await _db.GetPlanItem(planID, userID);
                editPlanItem.Year = year.Value;
                editPlanItem.Month = month.Value;
                List<TransactionCategory> transactionCategories = await _db.GetTransactionCategories(userID);
                List<TransactionType> transactionTypes = await _db.GetTransactionTypes();

                //get categories not already in the plan
                transactionCategories = transactionCategories.Where(cat => !planItems.Any(item => item.CategoryID == cat.ID) || cat.ID == editPlanItem.CategoryID).ToList();
                transactionCategories = transactionCategories.OrderBy(cat => cat.Name).ToList();
                TempData["TransactionCategories"] = transactionCategories;
                TempData["TransactionTypes"] = transactionTypes;
            }
            catch (Exception ex)
            {
                SentrySdk.CaptureException(ex);
            }
            return View("EditPlanItem", editPlanItem);
        }

        public async Task<IActionResult> AddNewPlanItem(PlanItem planItem)
        {
            try
            {
                await _db.InsertPlanItem(planItem);
            }
            catch (Exception ex)
            {
                SentrySdk.CaptureException(ex);
            }
            return RedirectToAction("Index", new { budgetID = planItem.BudgetID, year = planItem.Year, month = planItem.Month });
        }
        public async Task<IActionResult> EditExistingPlanItem(PlanItem planItem)
        {
            try
            {
                await _db.UpdatePlanItem(planItem);
            }
            catch (Exception ex)
            {
                SentrySdk.CaptureException(ex);
            }
            return RedirectToAction("Index", new { budgetID = planItem.BudgetID, year = planItem.Year, month = planItem.Month });
        }

        public async Task<IActionResult> DeletePlanItem(Guid budgetID, Guid planID, int? year, int? month)
        {
            if (budgetID == Guid.Empty || planID == Guid.Empty || year == null || month == null)
                return RedirectToAction("Index", "Home");
            try
            {
                await _db.DeletePlanItem(planID);
            }
            catch (Exception ex)
            {
                SentrySdk.CaptureException(ex);
            }
            return RedirectToAction("Index", new { budgetID, year, month });
        }
    }
}


/**
Plan

- category id, budget id, amount, month, year

*/