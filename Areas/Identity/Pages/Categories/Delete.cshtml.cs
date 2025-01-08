using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Inzynierka.Models;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Inzynierka.Services;

namespace Inzynierka.Areas.Identity.Pages.Categories
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
        public Category Category { get; set; }

        public List<Category> CategoryDetails { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            // Get the current user's ID
            var currentUserId = _userManager.GetUserId(User);

            // Fetch the category
            Category = await _context.Categories
                .FirstOrDefaultAsync(c => c.CategoryId == id && c.UserId == currentUserId);

            if (Category == null)
            {
                return NotFound();
            }

            // Prepare category details for the EJ2 Grid
            CategoryDetails = new List<Category> { Category };

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

            // Fetch the category
            var category = await _context.Categories
                .FirstOrDefaultAsync(c => c.CategoryId == id && c.UserId == currentUserId);

            if (category == null)
            {
                return NotFound();
            }

            // Delete the category
            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();

            // Redirect to the Index page after deletion
            return RedirectToPage("./Index");
        }
    }
}