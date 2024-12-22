using Microsoft.AspNetCore.Mvc;

namespace Inzynierka.Areas.Identity.Controllers
{
    public class DashboardController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
