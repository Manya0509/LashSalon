using LashBooking.Domain.Entities;       
using LashBooking.Domain.Interfaces;     
using Microsoft.AspNetCore.Mvc;

namespace LashBooking.Web.MVC.Controllers
{
    public class AboutController : Controller
    {
        private readonly IRepository<AboutInfo> _aboutInfos;

        public AboutController(IRepository<AboutInfo> aboutInfos)
        {
            _aboutInfos = aboutInfos;
        }

        // GET: /About
        public async Task<IActionResult> Index()
        {
            // Получаем все записи из таблицы (там 0 или 1 запись)
            var all = await _aboutInfos.GetAllAsync();

            // Берём первую запись или null, если таблица пуста
            var info = all.FirstOrDefault();

            // Передаём объект AboutInfo в View как модель
            // View получит его через @Model
            return View(info);
        }
    }
}
