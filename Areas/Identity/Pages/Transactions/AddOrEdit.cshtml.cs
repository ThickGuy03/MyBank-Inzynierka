using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Inzynierka.Models;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Inzynierka.Services;

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
        public Transaction Transaction { get; set; } = new Transaction();

        public IEnumerable<SelectListItem> Categories { get; set; }
        public List<SelectListItem> RecurrenceFrequencies { get; set; } = new List<SelectListItem>
        {
            new SelectListItem { Value = "Weekly", Text = "Weekly" },
            new SelectListItem { Value = "Monthly", Text = "Monthly" },
            new SelectListItem { Value = "Yearly", Text = "Yearly" }
        };

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return RedirectToPage("/Account/Login");
            }

            Categories = _context.Categories
                .Where(c => c.UserId == currentUser.Id)
                .Select(c => new SelectListItem
                {
                    Value = c.CategoryId.ToString(),
                    Text = c.TitleWithIcon
                })
                .ToList();

            if (id == null)
            {
                Transaction = new Transaction
                {
                    Date = DateTime.Now,
                    Amount = 0
                };
            }
            else
            {
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
            var currentUser = await _userManager.GetUserAsync(User);
            Transaction.UserId = currentUser.Id;

            if (!Transaction.IsRecurring)
            {
                Transaction.RecurrenceFrequency = null;
                Transaction.RecurrenceEndDate = null;
            }

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