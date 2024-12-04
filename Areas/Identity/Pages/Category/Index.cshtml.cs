using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.Linq;
using Inzynierka.Models; // Ensure this is added for the Category model
using Inzynierka.Services;

namespace Inzynierka.Areas.Identity.Pages.Category
{
    public class IndexModel : PageModel
    {
        public IEnumerable<Inzynierka.Models.Category> Categories { get; private set; } // Fully qualify here

        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public void OnGet()
        {
            Categories = _context.Categories.ToList();
        }
    }
}