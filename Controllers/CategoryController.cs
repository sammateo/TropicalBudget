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
            string userID = UserUtility.GetUserID(User);
            List<TransactionCategory> transactionCategories = await _db.GetTransactionCategories(userID);
            return View("CategoryIndex", transactionCategories);
        }
        
        public IActionResult Add()
        {
            return View("NewCategory");
        }
        public async Task<IActionResult> Edit(Guid categoryID)
        {
            string userID = UserUtility.GetUserID(User);
            TransactionCategory transactionCategory = await _db.GetTransactionCategory(categoryID, userID);
            return View("EditCategory", transactionCategory);
        }

        public async Task<IActionResult> AddNewCategory(TransactionCategory newCategory)
        {
            string userID = UserUtility.GetUserID(User);
            newCategory.UserID = userID;
            await _db.InsertTransactionCategory(newCategory);
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> EditExistingCategory(TransactionCategory newCategory)
        {
            string userID = UserUtility.GetUserID(User);
            newCategory.UserID = userID;
            await _db.UpdateTransactionCategory(newCategory);
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> DeleteCategory(Guid categoryID)
        {
            await _db.DeleteTransactionCategory(categoryID);
            return RedirectToAction("Index");
        }
    }
}
