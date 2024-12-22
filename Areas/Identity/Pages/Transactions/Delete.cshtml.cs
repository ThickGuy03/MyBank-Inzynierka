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

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            // Get the logged-in user's ID
            var currentUserId = _userManager.GetUserId(User);

            // Fetch the transaction for the logged-in user
            Transaction = await _context.Transactions
                .Include(t => t.Category)
                .FirstOrDefaultAsync(m => m.TransactionId == id && m.UserId == currentUserId);

            if (Transaction == null)
            {
                return NotFound();
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            // Get the logged-in user's ID
            var currentUserId = _userManager.GetUserId(User);

            // Fetch the transaction for deletion
            var transactionToDelete = await _context.Transactions
                .FirstOrDefaultAsync(m => m.TransactionId == id && m.UserId == currentUserId);

            if (transactionToDelete == null)
            {
                return NotFound();
            }

            // Remove the transaction from the database
            _context.Transactions.Remove(transactionToDelete);
            await _context.SaveChangesAsync();

            // Redirect back to the Index page after deletion
            return RedirectToPage("./Index");
        }
    }
}
