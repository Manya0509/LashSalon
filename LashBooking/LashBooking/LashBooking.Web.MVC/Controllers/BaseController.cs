using Microsoft.AspNetCore.Mvc;
using LashBooking.Domain.Constants;

namespace LashBooking.Web.MVC.Controllers
{
    public class BaseController : Controller
    //  базовый контроллер от которого наследуются все остальные контроллеры. Содержит метод CatchException() — вызывает Logger.LogError() с форматом UserData*ErrorMsg*BrowserInfo. Все контроллеры теперь наследуют BaseController вместо Controller.
    {
        public ILogger Logger { get; set; }
        public string user;
        public string browserInfo;

        public BaseController(ILogger logger)
        { 
            Logger = logger; 
        }

        protected void InitRequestInfo()
        {
            // User-Agent — строка браузера
            // Пример: "Mozilla/5.0 (Windows NT 10.0; Win64; x64) Chrome/120.0"
            var userAgent = HttpContext.Request.Headers["User-Agent"].ToString();

            // IP адрес пользователя
            // X-Forwarded-For — реальный IP если запрос через Nginx/прокси
            var ip = HttpContext.Request.Headers["X-Forwarded-For"].ToString();
            if (string.IsNullOrEmpty(ip))
                ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";

            // Язык браузера
            var lang = HttpContext.Request.Headers["Accept-Language"].ToString();

            // Собираем всё в одну строку
            browserInfo = $"UA: {userAgent} | IP: {ip} | Lang: {lang}";

            // Текущий пользователь из сессии
            user = HttpContext.Session.GetString("ClientName") ?? "Anonymous";
        }

        public void CatchException(Exception e, string additionalInfo,
            int errorLevel = 4)
        {
            Logger.LogError(e, $"{user}*Error [{errorLevel}]: {additionalInfo}*{browserInfo}");

            TempData["ErrorOccurred"] = true;
            TempData["NeedReload"] = errorLevel >= ErrorLevel.Error;
            TempData["ErrorMessage"] = errorLevel switch
            {
                ErrorLevel.Warning => "Операция не выполнена. Проверьте введённые данные.",
                ErrorLevel.Error => "Произошла ошибка. Попробуйте ещё раз или обновите страницу.",
                ErrorLevel.Critical => "Сервис временно недоступен. Попробуйте позже.",
                _ => "Что-то пошло не так."
            };
        }
    }
}
