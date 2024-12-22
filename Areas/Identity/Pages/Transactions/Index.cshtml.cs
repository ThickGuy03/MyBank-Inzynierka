using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Inzynierka.Models;
using Inzynierka.Services;
using Microsoft.AspNetCore.Identity;
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
        }
    }
}
