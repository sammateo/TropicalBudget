using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TropicalBudget.Models;
using TropicalBudget.Services;

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
            List<TransactionCategory> transactionCategories = await _db.GetTransactionCategories();
            return View("CategoryIndex", transactionCategories);
        }
        
        public IActionResult Add()
        {
            return View("NewCategory");
        }
        public async Task<IActionResult> Edit(Guid categoryID)
        {
            TransactionCategory transactionCategory = await _db.GetTransactionCategory(categoryID);
            return View("EditCategory", transactionCategory);
        }

        public async Task<IActionResult> AddNewCategory(TransactionCategory newCategory)
        {
            await _db.InsertTransactionCategory(newCategory);
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> EditExistingCategory(TransactionCategory newCategory)
        {
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
