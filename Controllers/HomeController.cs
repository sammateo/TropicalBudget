using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using TestMVC.Models;
using TestMVC.Services;
using TestMVC.Utilities;

namespace TestMVC.Controllers
{
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
                if(month.Value > 12 || month.Value < 1)
                {
                    return RedirectToAction("Index");
                }
                startDate = new DateTime(year.Value, month.Value, 1, 0, 0, 0);
                endDate = startDate.AddMonths(1).AddSeconds(-1);
                currentMonth = $"{startDate.ToString("MMMM")}, {startDate.ToString("yyyy")}";
            }
            TempData["currentMonthString"] = currentMonth;
            TempData["startDate"] = startDate;
            List<Transaction> transactions = await _db.GetTransactions(startDate,endDate);
            return View(transactions);
        }
        
        public async Task<IActionResult> NewTransaction()
        {
            List<TransactionCategory> transactionCategories = await _db.GetTransactionCategories();
            List<TransactionSource> transactionSources = await _db.GetTransactionSources();
            List<TransactionType> transactionTypes = await _db.GetTransactionTypes();
            TempData["TransactionCategories"] = transactionCategories;
            TempData["TransactionSources"] = transactionSources;
            TempData["TransactionTypes"] = transactionTypes;
            Transaction newTransaction = new();
            newTransaction.TransactionDate = DateTime.Now;
            if(transactionTypes.Any(type => type.Name.Equals(TransactionUtility.TRANSACTION_TYPE_EXPENSE)))
            {
                newTransaction.TransactionTypeID = transactionTypes.FirstOrDefault(type => type.Name.Equals(TransactionUtility.TRANSACTION_TYPE_EXPENSE)).ID;
            }
            return View(newTransaction);
        }
        
        public async Task<IActionResult> EditTransaction(Guid transactionID)
        {
            List<TransactionCategory> transactionCategories = await _db.GetTransactionCategories();
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
            return RedirectToAction("Index");
        }
        public async Task<IActionResult> EditExistingTransaction(Transaction newTransaction)
        {
            await _db.UpdateTransaction(newTransaction);
            return RedirectToAction("Index");
        }
        
        public async Task<IActionResult> DeleteTransaction(Guid transactionID)
        {
            await _db.DeleteTransaction(transactionID);
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
