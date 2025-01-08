using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Inzynierka.Models;
using Inzynierka.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Inzynierka.Areas.Identity.Pages.Transactions
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public IndexModel(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public List<Transaction> Transactions { get; set; }

        public async Task OnGetAsync()
        {
            // Get the currently logged-in user's ID
            var currentUserId = _userManager.GetUserId(User);

            // Fetch only the transactions associated with the current user
            Transactions = await _context.Transactions
                .Include(t => t.Category)
                .Where(t => t.UserId == currentUserId)
                .ToListAsync();
            Console.WriteLine($"Loaded {Transactions.Count} transactions for user {currentUserId}.");
        }
        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            // Log the ID being passed
            Console.WriteLine($"Attempting to delete transaction with ID: {id}");

            var transaction = await _context.Transactions.FindAsync(id);

            if (transaction == null)
            {
                Console.WriteLine($"Transaction with ID {id} not found.");
                return NotFound();
            }

            // Ensure the transaction belongs to the current user
            var currentUserId = _userManager.GetUserId(User);
            if (transaction.UserId != currentUserId)
            {
                Console.WriteLine($"User {currentUserId} is not authorized to delete transaction {id}.");
                return Forbid();
            }

            _context.Transactions.Remove(transaction);
            await _context.SaveChangesAsync();

            Console.WriteLine($"Transaction with ID {id} deleted successfully.");

            return RedirectToPage();
        }
    }
}
