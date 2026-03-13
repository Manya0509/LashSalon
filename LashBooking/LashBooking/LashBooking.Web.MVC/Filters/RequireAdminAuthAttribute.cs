using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace LashBooking.Web.MVC.Filters
{
    /// <summary>
    /// Помечает метод как публичный — фильтр RequireAdminAuth его пропустит
    /// </summary>
    public class SkipRequireAdminAuthAttribute : Attribute { }

    /// <summary>
    /// Фильтр авторизации для администратора.
    /// Вешается на контроллер через [RequireAdminAuth].
    /// Методы с [SkipRequireAdminAuth] доступны без авторизации.
    /// </summary>
    public class RequireAdminAuthAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var hasSkip = context.ActionDescriptor.EndpointMetadata
                .OfType<SkipRequireAdminAuthAttribute>()
                .Any();
            // context.ActionDescriptor — описание метода который сейчас вызывается
            // .EndpointMetadata — список всех атрибутов на этом методе
            // .OfType<SkipRequireAdminAuthAttribute>() — фильтруем: ищем только наш атрибут
            // .Any() — есть хоть один такой атрибут? true/false

            if (hasSkip)
            {
                base.OnActionExecuting(context);
                // base — вызываем метод родительского класса (стандартное поведение)
                return;
                // Выходим — метод помечен как публичный, проверки не нужны
            }

            var isAdmin = context.HttpContext.Session.GetString("IsAdmin");
            // HttpContext — контекст текущего HTTP запроса
            // Session — временное хранилище данных пользователя на сервере
            // GetString("IsAdmin") — читаем значение по ключу "IsAdmin"
            // Если не авторизован или сессия истекла — вернёт null

            if (isAdmin != "true")
            // Если значение НЕ равно строке "true" (включая null) — не пускаем
            {
                context.Result = new RedirectToActionResult("Login", "Admin", null);
                // Устанавливаем результат запроса — редирект
                // "Login" — название метода, "Admin" — название контроллера
                // null — параметры маршрута (не нужны)
                // Итог: пользователь отправляется на /Admin/Login
                return;
                // Выходим — метод контроллера НЕ выполнится
            }

            base.OnActionExecuting(context);
            // Если дошли сюда — пользователь админ, пускаем дальше
            // Вызываем стандартное поведение и метод контроллера выполняется
        }
    }
}
