using Microsoft.AspNetCore.Mvc;

namespace BookStore_Client.Controllers
{
    public class DashboardController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
