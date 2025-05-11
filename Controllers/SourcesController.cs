using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TropicalBudget.Models;
using TropicalBudget.Services;
using TropicalBudget.Utilities;

namespace TropicalBudget.Controllers
{
    [Authorize]
    public class SourcesController : Controller
    {
        private readonly DatabaseService _db;
        public SourcesController(DatabaseService db)
        {
            _db = db;
        }
        public async Task<IActionResult> Index()
        {
            List<TransactionSource> transactionSources = new();
            try
            {
                string userID = UserUtility.GetUserID(User);
                transactionSources = await _db.GetTransactionSources(userID);
            }
            catch (Exception ex)
            {
                SentrySdk.CaptureException(ex);
            }
            return View("ViewTransactionSources", transactionSources);
        }
        public IActionResult Add()
        {
            return View("NewSource");
        }
        public async Task<IActionResult> Edit(Guid sourceID)
        {
            TransactionSource transactionSource = new();
            try
            {
                string userID = UserUtility.GetUserID(User);
                transactionSource = await _db.GetTransactionSource(sourceID, userID);
            }
            catch (Exception ex)
            {
                SentrySdk.CaptureException(ex);
            }
            return View("EditSource", transactionSource);
        }

        public async Task<IActionResult> AddNewSource(TransactionSource newSource)
        {
            try
            {
                string userID = UserUtility.GetUserID(User);
                newSource.UserId = userID;
                await _db.InsertTransactionSource(newSource);
            }
            catch (Exception ex)
            {
                SentrySdk.CaptureException(ex);
            }
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> EditExistingSource(TransactionSource newSource)
        {
            try
            {
                string userID = UserUtility.GetUserID(User);
                newSource.UserId = userID;
                await _db.UpdateTransactionSource(newSource);
            }
            catch (Exception ex)
            {
                SentrySdk.CaptureException(ex);
            }
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> DeleteSource(Guid sourceID)
        {
            try
            {
                string userID = UserUtility.GetUserID(User);
                await _db.DeleteTransactionSource(sourceID, userID);
            }
            catch (Exception ex)
            {
                SentrySdk.CaptureException(ex);
            }
            return RedirectToAction("Index");
        }
    }
}
