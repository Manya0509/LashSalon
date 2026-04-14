using Microsoft.AspNetCore.Mvc;
using LashBooking.Domain.Constants;
using LashBooking.Domain.Entities;
using LashBooking.Domain.Interfaces;

namespace LashBooking.Web.MVC.Controllers
{
    public class ServicesController : BaseController
    {
        private readonly IRepository<Service> _services;

        public ServicesController(
            IRepository<Service> services,
            ILogger logger) : base(logger)
        {
            _services = services;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                InitRequestInfo();

                var allServices = await _services.GetAllAsync();
                var activeServices = allServices
                    .Where(s => s.IsActive)
                    .OrderBy(s => s.Price)
                    .ToList();

                return View(activeServices);
            }
            catch (Exception ex)
            {
                CatchException(ex, "ServicesController/Index", ErrorLevel.Critical);
                return Content("Сервис временно недоступен. Попробуйте позже.");
            }
        }
    }
}
