using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TropicalBudget.Models;
using TropicalBudget.Services;
using TropicalBudget.Utilities;

namespace TropicalBudget.Controllers
{
    [Authorize]
    public class TransactionController : Controller
    {
        private readonly DatabaseService _db;
        public TransactionController(DatabaseService db)
        {
            _db = db;
        }

        public async Task<IActionResult> Index(Guid budgetID, int? year, int? month)
        {
            if (budgetID == Guid.Empty)
                return RedirectToAction("Index", "Home");

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
            Tuple<Guid, List<Transaction>> budgetTransactions = new(budgetID, transactions);
            return View("ViewTransactions", budgetTransactions);
        }

        public async Task<IActionResult> New(Guid budgetID)
        {
            string userID = UserUtility.GetUserID(User);
            List<TransactionCategory> transactionCategories = await _db.GetTransactionCategories(userID);
            List<TransactionSource> transactionSources = await _db.GetTransactionSources();
            List<TransactionType> transactionTypes = await _db.GetTransactionTypes();
            TempData["TransactionCategories"] = transactionCategories;
            TempData["TransactionSources"] = transactionSources;
            TempData["TransactionTypes"] = transactionTypes;
            Transaction newTransaction = new();
            newTransaction.BudgetID = budgetID;
            newTransaction.TransactionDate = DateTime.Now;
            if (transactionTypes.Any(type => type.Name.Equals(TransactionUtility.TRANSACTION_TYPE_EXPENSE)))
            {
                newTransaction.TransactionTypeID = transactionTypes.FirstOrDefault(type => type.Name.Equals(TransactionUtility.TRANSACTION_TYPE_EXPENSE)).ID;
            }
            return View("NewTransaction",newTransaction);
        }

        public async Task<IActionResult> EditTransaction(Guid transactionID)
        {
            string userID = UserUtility.GetUserID(User);
            List<TransactionCategory> transactionCategories = await _db.GetTransactionCategories(userID);
            List<TransactionSource> transactionSources = await _db.GetTransactionSources();
            List<TransactionType> transactionTypes = await _db.GetTransactionTypes();
            TempData["TransactionCategories"] = transactionCategories;
            TempData["TransactionSources"] = transactionSources;
            TempData["TransactionTypes"] = transactionTypes;
            Transaction editingTransaction = await _db.GetTransaction(transactionID);
            return View(editingTransaction);
        }


        public async Task<IActionResult> AddNewTransaction(Transaction newTransaction)
        {
            await _db.InsertTransaction(newTransaction);
            return RedirectToAction("Index",new {budgetID = newTransaction.BudgetID});
        }
        public async Task<IActionResult> EditExistingTransaction(Transaction newTransaction)
        {
            await _db.UpdateTransaction(newTransaction);
            return RedirectToAction("Index", new { budgetID = newTransaction.BudgetID });
        }

        public async Task<IActionResult> DeleteTransaction(Guid budgetID, Guid transactionID)
        {
            await _db.DeleteTransaction(transactionID);
            return RedirectToAction("Index", new { budgetID });
        }
    }
}
