using System.Globalization;
using CsvHelper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TropicalBudget.Models;
using TropicalBudget.Models.ViewModels;
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
            NewTransactionViewModel newTransactionViewModel = new();
            try
            {
                string userID = UserUtility.GetUserID(User);
                List<TransactionCategory> transactionCategories = await _db.GetTransactionCategories(userID);
                List<TransactionSource> transactionSources = await _db.GetTransactionSources(userID);
                List<TransactionType> transactionTypes = await _db.GetTransactionTypes();
                List<SavingsGoal> savingsGoals = await _db.GetSavingsGoals(budgetID);
                if (transactionTypes.Count == 2)
                {
                    transactionTypes.Add(new TransactionType()
                    {
                        ID = transactionTypes.Where(type => type.Name == TransactionUtility.TRANSACTION_TYPE_EXPENSE).First().ID,
                        Name = TransactionUtility.TRANSACTION_TYPE_SAVINGS_ADD
                    });
                    transactionTypes.Add(new TransactionType()
                    {
                        ID = transactionTypes.Where(type => type.Name == TransactionUtility.TRANSACTION_TYPE_INCOME).First().ID,
                        Name = TransactionUtility.TRANSACTION_TYPE_SAVINGS_WITHDRAW
                    });
                }


                newTransaction.BudgetID = budgetID;
                newTransaction.TransactionDate = DateTime.Now.ToLocalTime();
                if (transactionTypes.Any(type => type.Name.Equals(TransactionUtility.TRANSACTION_TYPE_EXPENSE)))
                {
                    newTransaction.TransactionTypeID = transactionTypes.FirstOrDefault(type => type.Name.Equals(TransactionUtility.TRANSACTION_TYPE_EXPENSE)).ID;
                }

                newTransactionViewModel = new NewTransactionViewModel
                {
                    TransactionCategories = transactionCategories,
                    TransactionSources = transactionSources,
                    TransactionTypes = transactionTypes,
                    SavingsGoals = savingsGoals,
                    NewTransaction = newTransaction
                };
            }
            catch (Exception ex)
            {
                SentrySdk.CaptureException(ex);
            }



            return View("NewTransaction", newTransactionViewModel);
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
                List<SavingsGoal> savingsGoals = await _db.GetSavingsGoals(editingTransaction.BudgetID);
                TempData["SavingsGoals"] = savingsGoals;

                if (editingTransaction.TransactionTypeID == transactionTypes.Where(type => type.Name == TransactionUtility.TRANSACTION_TYPE_INCOME).First().ID)
                {
                    editingTransaction.UiTransactionType = editingTransaction.IsSavings ? "withdraw_savings" : "income";
                }
                if (editingTransaction.TransactionTypeID == transactionTypes.Where(type => type.Name == TransactionUtility.TRANSACTION_TYPE_EXPENSE).First().ID)
                {
                    editingTransaction.UiTransactionType = editingTransaction.IsSavings ? "add_savings" : "expense";
                }
            }
            catch (Exception ex)
            {
                SentrySdk.CaptureException(ex);
            }
            return View(editingTransaction);
        }

        private async Task<Transaction> ManageTransactionTypeDetails(Transaction transaction)
        {

            if (transaction.UiTransactionType != null)
            {
                List<TransactionType> transactionTypes = await _db.GetTransactionTypes();

                //set transaction type id
                if (transaction.UiTransactionType == "income" || transaction.UiTransactionType == "withdraw_savings")
                {
                    transaction.TransactionTypeID = transactionTypes.Where(type => type.Name == TransactionUtility.TRANSACTION_TYPE_INCOME).FirstOrDefault().ID;
                }
                else
                {
                    transaction.TransactionTypeID = transactionTypes.Where(type => type.Name == TransactionUtility.TRANSACTION_TYPE_EXPENSE).FirstOrDefault().ID;
                }
                //check if it is a savings transaction and set savings goal id to null
                if (transaction.UiTransactionType == "income" || transaction.UiTransactionType == "expense")
                {
                    transaction.SavingsGoalID = Guid.Empty;
                }
                else
                {
                    transaction.CategoryID = Guid.Empty;
                }
            }
            return transaction;
        }


        public async Task<IActionResult> AddNewTransaction(Transaction newTransaction)
        {
            try
            {
                newTransaction = await ManageTransactionTypeDetails(newTransaction);
                await _db.InsertTransaction(newTransaction);
            }
            catch (Exception ex)
            {
                SentrySdk.CaptureException(ex);
            }
            return RedirectToAction("Index", new { budgetID = newTransaction.BudgetID });
        }
        public async Task<IActionResult> EditExistingTransaction(Transaction newTransaction)
        {
            try
            {
                newTransaction = await ManageTransactionTypeDetails(newTransaction);

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
                filePath = $"Transactions - {startDate.ToString("MMM, yyyy")}.csv";
                List<Transaction> transactions = await _db.GetTransactions(budgetID, startDate, endDate);
                transactions = transactions.OrderByDescending(x => x.CreatedAt).ToList();
                List<TransactionExport> transactionExports = await TransactionUtility.ConvertTransactionsToExportTransactions(transactions);
                using (StreamWriter writer = new(stream, leaveOpen: true))
                {
                    CsvWriter csv = new(writer, new CultureInfo("en-US"));
                    csv.WriteRecords(transactionExports);
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
