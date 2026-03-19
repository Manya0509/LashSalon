using Microsoft.AspNetCore.Mvc;

namespace LashBooking.Web.MVC.Controllers
{
    public class AboutController : Controller
    {
        public IActionResult Index()
        {
            return View();    // возвращает Views/About/Index.cshtml
        }
    }
}
