using Microsoft.AspNetCore.Mvc;

namespace BookStore_Client.Controllers
{
    public class OrderController : Controller
    {
        public IActionResult Cart()
        {
            return View();
        }
    }
}
