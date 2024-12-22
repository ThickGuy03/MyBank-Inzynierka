using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Inzynierka.Models;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;
using Inzynierka.Services;
using Microsoft.EntityFrameworkCore;

namespace Inzynierka.Areas.Identity.Pages.Categories
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
        public Category Category { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                // If no category id is passed, it's a new category, initialize empty.
                Category = new Category();
                return Page();
            }

            // Edit existing category
            Category = await _context.Categories.FindAsync(id);

            if (Category == null)
            {
                return NotFound();
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return RedirectToPage("/Account/Login");
            }

            if (Category.CategoryId != 0)
            {
                var existingCategory = await _context.Categories
                    .FirstOrDefaultAsync(c => c.CategoryId == Category.CategoryId && c.UserId == currentUser.Id);

                if (existingCategory != null)
                {
                    existingCategory.Title = Category.Title;
                    existingCategory.Icon = Category.Icon;
                    existingCategory.Type = Category.Type;
                    _context.Update(existingCategory);
                }
            }
            else
            {
                Category.UserId = currentUser.Id;
                _context.Categories.Add(Category);
            }

            await _context.SaveChangesAsync();
            return RedirectToPage("./Index");
        }

    }
}
