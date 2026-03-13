using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace LashBooking.Web.MVC.Filters
{
    /// <summary>
    /// Фильтр авторизации для клиентов.
    /// Вешается на контроллер или метод через [RequireClientAuth]
    /// </summary>
    public class RequireClientAuthAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var session = context.HttpContext.Session;
            // Сохраняем сессию в переменную чтобы не писать длинный путь дважды
            // Session — хранилище данных конкретного пользователя на сервере

            var clientId = session.GetInt32("ClientId");
            // Читаем из сессии числовой ID клиента
            // GetInt32 — возвращает int? (nullable int)
            // Если клиент не входил → вернёт null
            // Отличие от админа: там GetString("IsAdmin"), здесь GetInt32("ClientId")
            // ID числовой потому что это ключ из базы данных

            if (clientId == null)
            // null означает: клиент не авторизован или сессия истекла
            {
                context.Result = new RedirectToActionResult("Login", "Auth", null);
                // Редирект на /Auth/Login — страница входа для клиентов
                // "Login" — метод, "Auth" — контроллер, null — без параметров
                // Метод контроллера НЕ выполнится
                return;
                // Выходим из фильтра
            }

            base.OnActionExecuting(context);
            // Если clientId есть — клиент авторизован, пускаем дальше
            // Метод контроллера выполняется как обычно
        }
    }
}
