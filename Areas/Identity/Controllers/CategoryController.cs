using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Inzynierka.Models;
using Inzynierka.Services;
using System.Reflection;

namespace Inzynierka.Areas.Identity.Controllers
{
    [Area("Identity")]
    public class CategoryController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public CategoryController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Category
        public async Task<IActionResult> Index()
        {
            var categories = await _context.Categories.ToListAsync();
            return View(categories);
        }

        // GET: Category/AddOrEdit
        public IActionResult AddOrEdit(int id=0)
        {
            if(id==0)
                return View(new Category());
            else
                return  View(_context.Categories.Find(id));
        }

        // POST: Categories/AddOrEdit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddOrEdit([Bind("CategoryId,Title,Icon,Type")] Category category)
        {
            // Get the current user
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                // Handle case where user is not authenticated
                return RedirectToAction("Login", "Account", new { area = "Identity" });
            }

            Console.WriteLine($"User ID: {currentUser.Id}");


            category.UserId = currentUser.Id;

            if (ModelState.IsValid)
            {
                if(category.CategoryId==0)
                    _context.Categories.Add(category);  
                else
                    _context.Categories.Update(category);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(category);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category != null)
            {
                _context.Categories.Remove(category);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

    }
}
