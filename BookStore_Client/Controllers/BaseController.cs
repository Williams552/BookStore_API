using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace BookStore_Client.Controllers
{
    public class BaseController : Controller
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            // Kiểm tra xem người dùng đã đăng nhập chưa
            var role = HttpContext.Session.GetInt32("Role");

            if (!role.HasValue)
            {
                // Nếu chưa đăng nhập, chuyển hướng đến trang đăng nhập
                context.Result = new RedirectToActionResult("Login", "User", new { area = "api/Auth" });
                return;
            }

            // Kiểm tra quyền truy cập: chỉ admin (Role = 1) được phép truy cập
            if (role != 1)
            {
                // Nếu không phải admin, chuyển hướng về trang Home Page
                context.Result = new RedirectResult("https://localhost:7106/");
                return;
            }

            base.OnActionExecuting(context);
        }
    }
}