using Microsoft.AspNetCore.Mvc;

namespace PROEL4W_MVC_Kaijenson_Motor_Shop.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            if (HttpContext.Session.GetInt32("UserId") != null)
                return RedirectToAction("Index", "Dashboard");

            return RedirectToAction("Login", "Account");
        }
    }
}
