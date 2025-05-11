using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TropicalBudget.Models;
using TropicalBudget.Services;
using TropicalBudget.Utilities;

namespace TropicalBudget.Controllers
{
    [Authorize]
    public class CategoryController : Controller
    {
        private readonly DatabaseService _db;
        public CategoryController(DatabaseService db)
        {
            _db = db;
        }
    
        public async Task<IActionResult> Index()
        {
            List<TransactionCategory> transactionCategories = new();
            try
            {
                string userID = UserUtility.GetUserID(User);
                transactionCategories = await _db.GetTransactionCategories(userID);
            }
            catch (Exception ex)
            {
                SentrySdk.CaptureException(ex);
            }
            return View("CategoryIndex", transactionCategories);
        }
        
        public IActionResult Add()
        {
            return View("NewCategory");
        }
        public async Task<IActionResult> Edit(Guid categoryID)
        {
            TransactionCategory transactionCategory = new();
            try
            {
                string userID = UserUtility.GetUserID(User);
                transactionCategory = await _db.GetTransactionCategory(categoryID, userID);
            }
            catch (Exception ex)
            {
                SentrySdk.CaptureException(ex);
            }
            return View("EditCategory", transactionCategory);
        }

        public async Task<IActionResult> AddNewCategory(TransactionCategory newCategory)
        {
            try
            {
                string userID = UserUtility.GetUserID(User);
                newCategory.UserID = userID;
                await _db.InsertTransactionCategory(newCategory);
            }
            catch (Exception ex)
            {
                SentrySdk.CaptureException(ex);
            }
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> EditExistingCategory(TransactionCategory newCategory)
        {
            try
            {
                string userID = UserUtility.GetUserID(User);
                newCategory.UserID = userID;
                await _db.UpdateTransactionCategory(newCategory);
            }
            catch (Exception ex)
            {
                SentrySdk.CaptureException(ex);
            }
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> DeleteCategory(Guid categoryID)
        {
            try
            {
                await _db.DeleteTransactionCategory(categoryID);
            }
            catch (Exception ex)
            {
                SentrySdk.CaptureException(ex);
            }
            return RedirectToAction("Index");
        }
    }
}
