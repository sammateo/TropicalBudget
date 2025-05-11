using System.Globalization;
using CsvHelper;
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
            
            return View("ViewTransactions", budgetTransactions);
        }

        public async Task<IActionResult> New(Guid budgetID)
        {
            Transaction newTransaction = new();
            try
            {
                string userID = UserUtility.GetUserID(User);
                List<TransactionCategory> transactionCategories = await _db.GetTransactionCategories(userID);
                List<TransactionSource> transactionSources = await _db.GetTransactionSources(userID);
                List<TransactionType> transactionTypes = await _db.GetTransactionTypes();
                TempData["TransactionCategories"] = transactionCategories;
                TempData["TransactionSources"] = transactionSources;
                TempData["TransactionTypes"] = transactionTypes;
                newTransaction.BudgetID = budgetID;
                newTransaction.TransactionDate = DateTime.Now;
                if (transactionTypes.Any(type => type.Name.Equals(TransactionUtility.TRANSACTION_TYPE_EXPENSE)))
                {
                    newTransaction.TransactionTypeID = transactionTypes.FirstOrDefault(type => type.Name.Equals(TransactionUtility.TRANSACTION_TYPE_EXPENSE)).ID;
                }
            }
            catch (Exception ex)
            {
                SentrySdk.CaptureException(ex);
            }
            
            return View("NewTransaction",newTransaction);
        }

        public async Task<IActionResult> EditTransaction(Guid transactionID)
        {
            Transaction editingTransaction = new();
            try
            {
                string userID = UserUtility.GetUserID(User);
                List<TransactionCategory> transactionCategories = await _db.GetTransactionCategories(userID);
                List<TransactionSource> transactionSources = await _db.GetTransactionSources(userID);
                List<TransactionType> transactionTypes = await _db.GetTransactionTypes();
                TempData["TransactionCategories"] = transactionCategories;
                TempData["TransactionSources"] = transactionSources;
                TempData["TransactionTypes"] = transactionTypes;
                editingTransaction = await _db.GetTransaction(transactionID);
            }
            catch (Exception ex)
            {
                SentrySdk.CaptureException(ex);
            }
            return View(editingTransaction);
        }


        public async Task<IActionResult> AddNewTransaction(Transaction newTransaction)
        {
            try
            {
                await _db.InsertTransaction(newTransaction);
            }
            catch (Exception ex)
            {
                SentrySdk.CaptureException(ex);
            }
            return RedirectToAction("Index",new {budgetID = newTransaction.BudgetID});
        }
        public async Task<IActionResult> EditExistingTransaction(Transaction newTransaction)
        {
            try
            {
                await _db.UpdateTransaction(newTransaction);
            }
            catch (Exception ex)
            {
                SentrySdk.CaptureException(ex);
            }
            return RedirectToAction("Index", new { budgetID = newTransaction.BudgetID });
        }

        public async Task<IActionResult> DeleteTransaction(Guid budgetID, Guid transactionID)
        {
            try
            {
                await _db.DeleteTransaction(transactionID);
            }
            catch (Exception ex)
            {
                SentrySdk.CaptureException(ex);
            }
            return RedirectToAction("Index", new { budgetID });
        }

        public async Task<IActionResult> Export(Guid budgetID, int? year, int? month)
        {
            MemoryStream stream = new();
            string filePath = $"Transactions.csv";
            try
            {
                DateTime startDate = new DateTime(year.Value, month.Value, 1, 0, 0, 0);
                DateTime endDate = startDate.AddMonths(1).AddSeconds(-1);
                List<Transaction> transactions = await _db.GetTransactions(budgetID, startDate, endDate);
                using (StreamWriter writer = new(stream, leaveOpen:true))
                {
                    CsvWriter csv = new(writer, new CultureInfo("en-US"));
                    csv.WriteRecords(transactions);
                }
                stream.Position = 0;
            }
            catch (Exception ex)
            {
                SentrySdk.CaptureException(ex);
            }
            return File(stream, "application/octet-stream", filePath);
        }
    }
}
