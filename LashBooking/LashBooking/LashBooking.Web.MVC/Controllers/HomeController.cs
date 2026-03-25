using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Diagnostics;
using LashBooking.Domain.Constants;

namespace LashBooking.Web.MVC.Controllers
{
    public class HomeController : BaseController
    {
        public HomeController(ILogger logger) : base(logger)
        {
        }

        public IActionResult Index()
        {
            try
            {
                InitRequestInfo();

                var clientName = HttpContext.Session.GetString("ClientName");
                var isAuthenticated = !string.IsNullOrEmpty(clientName);

                ViewBag.ClientName = clientName ?? "";
                ViewBag.IsAuthenticated = isAuthenticated;

                return View();
            }
            catch (Exception ex)
            {
                // Ошибка главной страницы — Critical, страница должна всегда работать
                CatchException(ex, "HomeController/Index", ErrorLevel.Critical);
                return Content("Сервис временно недоступен. Попробуйте позже.");
            }
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            try
            {
                var exceptionFeature = HttpContext.Features.Get<IExceptionHandlerPathFeature>();

                ViewBag.Path = exceptionFeature?.Path ?? "Неизвестный путь";
                ViewBag.Message = exceptionFeature?.Error?.Message ?? "Неизвестная ошибка";

                return View();
            }
            catch (Exception ex)
            {
                // Ошибка на странице ошибок — Critical
                CatchException(ex, "HomeController/Error", ErrorLevel.Critical);
                return Content("Произошла критическая ошибка.");
            }
        }
    }
}
