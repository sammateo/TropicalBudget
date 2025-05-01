using Microsoft.AspNetCore.Mvc;
using TropicalBudget.Models;
using TropicalBudget.Services;
using TropicalBudget.Utilities;

namespace TropicalBudget.Controllers
{
    public class SourcesController : Controller
    {
        private readonly DatabaseService _db;
        public SourcesController(DatabaseService db)
        {
            _db = db;
        }
        public async Task<IActionResult> Index()
        {
            string userID = UserUtility.GetUserID(User);
            List<TransactionSource> transactionSources = await _db.GetTransactionSources(userID);
            return View("ViewTransactionSources", transactionSources);
        }
        public IActionResult Add()
        {
            return View("NewSource");
        }
        public async Task<IActionResult> Edit(Guid sourceID)
        {
            string userID = UserUtility.GetUserID(User);
            TransactionSource transactionSource = await _db.GetTransactionSource(sourceID, userID);
            return View("EditSource", transactionSource);
        }

        public async Task<IActionResult> AddNewSource(TransactionSource newSource)
        {
            string userID = UserUtility.GetUserID(User);
            newSource.UserId = userID;
            await _db.InsertTransactionSource(newSource);
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> EditExistingSource(TransactionSource newSource)
        {
            string userID = UserUtility.GetUserID(User);
            newSource.UserId = userID;
            await _db.UpdateTransactionSource(newSource);
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> DeleteSource(Guid sourceID)
        {
            string userID = UserUtility.GetUserID(User);
            await _db.DeleteTransactionSource(sourceID, userID);
            return RedirectToAction("Index");
        }
    }
}
