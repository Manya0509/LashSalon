using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Diagnostics;

namespace LashBooking.Web.MVC.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        // Этот метод вызывается когда пользователь заходит на главную страницу сайта (/)
        // IActionResult — означает что метод вернёт какой-то результат (страницу, редирект и т.д.)
        {
            var clientName = HttpContext.Session.GetString("ClientName");   // Читаем из сессии (временное хранилище на сервере для конкретного пользователя) значение по ключу "ClientName".
                                                                            //Если пользователь не авторизован — вернёт null.
            var isAuthenticated = !string.IsNullOrEmpty(clientName);    // Проверяем: если имя есть (не пустое и не null) — пользователь авторизован (true), иначе — нет (false).
            ViewBag.ClientName = clientName ?? "";                      // если clientName = null → передаём пустую строку "", иначе передаём имя
            ViewBag.IsAuthenticated = isAuthenticated;                  // Передаём в HTML true или false — вошёл пользователь или нет
            return View();                                              // Возвращаем HTML-страницу пользователю
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        // Это атрибут — инструкция для браузера
        // Duration = 0 — не хранить страницу в кэше ни секунды
        // NoStore = true — вообще не сохранять
        // Нужно чтобы страница ошибки всегда была свежей, а не старой из кэша
        public IActionResult Error()        // Этот метод вызывается автоматически когда в приложении происходит любая ошибка
        {
            var exceptionFeature = HttpContext.Features.Get<IExceptionHandlerPathFeature>();
            // HttpContext.Features — набор дополнительных данных о текущем запросе
            // Get<IExceptionHandlerPathFeature>() — достаём информацию об ошибке которая произошла
            // Если ошибки не было (зашли на /Error напрямую) — вернёт null

            ViewBag.Path = exceptionFeature?.Path ?? "Неизвестный путь";
            // exceptionFeature? — знак вопроса защищает от null (если null — не упадёт, вернёт null)
            // .Path — адрес страницы где произошла ошибка, например "/Booking/Create"
            // ?? "Неизвестный путь" — если Path = null, покажем этот текст
            // Передаём путь в HTML для отображения пользователю

            ViewBag.Message = exceptionFeature?.Error?.Message ?? "Неизвестная ошибка";
            // exceptionFeature? — защита от null первого уровня
            // .Error? — защита от null второго уровня (может не быть объекта ошибки)
            // .Message — текст ошибки, например "Объект не найден" или "Нет подключения к БД"
            // ?? "Неизвестная ошибка" — текст по умолчанию если сообщения нет

            return View();
            // Возвращаем HTML-страницу с ошибкой
            // ASP.NET найдёт файл Views/Home/Error.cshtml и покажет его пользователю
            // На этой странице будут доступны ViewBag.Path и ViewBag.Message
        }
    }
}
