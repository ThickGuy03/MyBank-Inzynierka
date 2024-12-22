using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using System.Linq;
using Inzynierka.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Inzynierka.Services;

namespace Inzynierka.Areas.Identity.Pages.Categories
{
    public class IndexModel : PageModel
    {
        public IEnumerable<Category> Categories { get; private set; }

        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public IndexModel(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task OnGet()
        {
            var currentUser = await _userManager.GetUserAsync(User);

            if (currentUser != null)
            {
                Categories = await _context.Categories
                    .AsNoTracking()
                    .Where(c => c.UserId == currentUser.Id)
                    .ToListAsync();
            }
            else
            {
                Categories = new List<Category>();
            }
        }

        // Delete handler
        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            var category = await _context.Categories.FindAsync(id);

            if (category == null)
            {
                return NotFound();
            }

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();

            return RedirectToPage("./Index"); // Redirect to reload updated data
        }
    }
}
