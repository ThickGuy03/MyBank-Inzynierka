using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Inzynierka.Models;
using Microsoft.AspNetCore.Identity;
using Inzynierka.Services;

namespace Inzynierka.Areas.Identity.Controllers
{
    [Area("Identity")]
    public class TransactionsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public TransactionsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Identity/Transactions
        public async Task<IActionResult> Index()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return RedirectToAction("Login", "Account", new { area = "Identity" });
            }

            var transactions = await _context.Transactions
                .Include(t => t.Category)
                .Where(t => t.Category.UserId == currentUser.Id) // Ensure transactions belong to the logged-in user
                .ToListAsync();

            return View(transactions);
        }

        // GET: Identity/Transactions/AddOrEdit
        public async Task<IActionResult> AddOrEdit(int id=0)
        {
            await PopulateCategoriesDropdownAsync();
            if(id==0)
                return View(new Transaction());
            else
                return View(_context.Transactions.Find(id));
        }

        // POST: Identity/Transactions/AddOrEdit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddOrEdit([Bind("TransactionId,CategoryId,Amount,Note,Date")] Transaction transaction)
        {
            if (!ModelState.IsValid)
            {
                await PopulateCategoriesDropdownAsync(transaction.CategoryId);
                return View(transaction);
            }

            var category = await _context.Categories.FindAsync(transaction.CategoryId);
            if (category == null)
            {
                ModelState.AddModelError("CategoryId", "Invalid category.");
                await PopulateCategoriesDropdownAsync(transaction.CategoryId);
                return View(transaction);
            }

            if (transaction.TransactionId == 0)
            {
                _context.Transactions.Add(transaction);
            }
            else
            {
                _context.Transactions.Update(transaction);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: Identity/Transactions/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var transaction = await GetTransactionForUser(id.Value);
            if (transaction == null)
            {
                return NotFound();
            }

            return View(transaction);
        }

        // POST: Identity/Transactions/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var transaction = await GetTransactionForUser(id);
            if (transaction == null)
            {
                return NotFound();
            }

            _context.Transactions.Remove(transaction);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private async Task<Transaction> GetTransactionForUser(int transactionId)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return null;
            }

            return await _context.Transactions
                .Include(t => t.Category)
                .FirstOrDefaultAsync(t => t.TransactionId == transactionId && t.Category.UserId == currentUser.Id);
        }

        private async Task PopulateCategoriesDropdownAsync(int? selectedCategoryId = null)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var categories = await _context.Categories
                .Where(c => c.UserId == currentUser.Id)
                .ToListAsync();

            ViewData["CategoryId"] = new SelectList(categories, "CategoryId", "Title", selectedCategoryId);
        }
    }
}
