using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Inzynierka.Models;

using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Inzynierka.Services;

namespace Inzynierka.Areas.Identity.Pages.Transactions
{
    public class DeleteModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public DeleteModel(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [BindProperty]
        public Transaction Transaction { get; set; }

        public List<Transaction> TransactionDetails { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            // Get the current user's ID
            var currentUserId = _userManager.GetUserId(User);

            // Fetch the transaction
            Transaction = await _context.Transactions
                .Include(t => t.Category) // Include related category
                .FirstOrDefaultAsync(t => t.TransactionId == id && t.UserId == currentUserId);

            if (Transaction == null)
            {
                return NotFound();
            }

            // Prepare transaction details for the EJ2 Grid
            TransactionDetails = new List<Transaction> { Transaction };

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            // Get the current user's ID
            var currentUserId = _userManager.GetUserId(User);

            // Fetch the transaction
            var transaction = await _context.Transactions
                .FirstOrDefaultAsync(t => t.TransactionId == id && t.UserId == currentUserId);

            if (transaction == null)
            {
                return NotFound();
            }

            // Delete the transaction
            _context.Transactions.Remove(transaction);
            await _context.SaveChangesAsync();

            // Redirect to the Index page after deletion
            return RedirectToPage("./Index");
        }
    }
}