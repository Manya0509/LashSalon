using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Diagnostics;
using LashBooking.Domain.Constants;
using LashBooking.Domain.Entities;
using LashBooking.Domain.Interfaces;

namespace LashBooking.Web.MVC.Controllers
{
    public class HomeController : BaseController
    {
        private readonly IRepository<Service> _services;
        private readonly IRepository<AboutInfo> _aboutInfos;

        public HomeController(
            IRepository<Service> services,
            IRepository<AboutInfo> aboutInfos,
            ILogger logger) : base(logger)
        {
            _services = services;
            _aboutInfos = aboutInfos;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                InitRequestInfo();

                var clientName = HttpContext.Session.GetString("ClientName");
                var isAuthenticated = !string.IsNullOrEmpty(clientName);

                ViewBag.ClientName = clientName ?? "";
                ViewBag.IsAuthenticated = isAuthenticated;

                var allServices = await _services.GetAllAsync();
                var activeServices = allServices.Where(s => s.IsActive).ToList();
                ViewBag.MinPrice = activeServices.Any() ? activeServices.Min(s => s.Price) : 0;

                // Загружаем данные "О себе" для контактов и имени на главной
                var allAbout = await _aboutInfos.GetAllAsync();
                ViewBag.AboutInfo = allAbout.FirstOrDefault();

                return View();
            }
            catch (Exception ex)
            {
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
                CatchException(ex, "HomeController/Error", ErrorLevel.Critical);
                return Content("Произошла критическая ошибка.");
            }
        }
    }
}