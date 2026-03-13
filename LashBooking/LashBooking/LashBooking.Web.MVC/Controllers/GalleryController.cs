using Microsoft.AspNetCore.Mvc;

namespace LashBooking.Web.MVC.Controllers
{
    public class GalleryController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
