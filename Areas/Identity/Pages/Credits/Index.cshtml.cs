using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Inzynierka.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using Inzynierka.Services;

namespace Inzynierka.Areas.Identity.Pages.Credits
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public List<Credit> Credits { get; set; }

        public async Task OnGetAsync()
        {
            // Fetch all available credits from the database
            Credits = await _context.Credits.ToListAsync();
        }
    }
}