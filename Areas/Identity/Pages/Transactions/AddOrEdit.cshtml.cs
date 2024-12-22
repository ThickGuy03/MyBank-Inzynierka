using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Inzynierka.Models;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;
using Inzynierka.Services;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Inzynierka.Areas.Identity.Pages.Transactions
{
    public class AddOrEditModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public AddOrEditModel(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [BindProperty]
        public Transaction Transaction { get; set; } = new Transaction(); // Initialize Transaction

        public IEnumerable<SelectListItem> Categories { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            // Get the current user
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return RedirectToPage("/Account/Login");
            }

            // Load categories for the dropdown
            Categories = _context.Categories
                .Where(c => c.UserId == currentUser.Id)
                .Select(c => new SelectListItem
                {
                    Value = c.CategoryId.ToString(),
                    Text = c.TitleWithIcon
                })
                .ToList();

            // If id is null, create a new transaction
            if (id == null)
            {
                Transaction = new Transaction
                {
                    Date = DateTime.Now // Default value for the date field
                };
            }
            else
            {
                // Load the transaction from the database
                Transaction = await _context.Transactions.FindAsync(id);
                if (Transaction == null)
                {
                    return NotFound();
                }
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            /*if (!ModelState.IsValid)
            {
                // Reload categories if validation fails
                var currentUser = await _userManager.GetUserAsync(User);
                Categories = _context.Categories
                    .Where(c => c.UserId == currentUser.Id)
                    .Select(c => new SelectListItem
                    {
                        Value = c.CategoryId.ToString(),
                        Text = c.Title
                    })
                    .ToList();

                return Page();
            }*/

            var currentUser2 = await _userManager.GetUserAsync(User);
            Transaction.UserId = currentUser2.Id;

            if (Transaction.TransactionId == 0)
            {
                _context.Transactions.Add(Transaction);
            }
            else
            {
                _context.Transactions.Update(Transaction);
            }

            await _context.SaveChangesAsync();
            return RedirectToPage("./Index");
        }
    }
}
