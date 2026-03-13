using Microsoft.VisualStudio.TestPlatform.ObjectModel.DataCollection;

namespace LashBooking.Web.MVC.Models
{
    public class ErrorViewModel
    // Модель для страницы ошибки — передаёт данные об ошибке в View
    {
    public string? RequestId { get; set; }
    // ID текущего HTTP-запроса
    // Например: "0HN5L7QKII02D:00000001"
    // ? — nullable, может быть null если ID не удалось получить
    // Используется для диагностики — по этому ID можно найти ошибку в логах

    public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    // => — это вычисляемое свойство (expression-bodied property)
    // Нет { get; set; } — значение вычисляется автоматически каждый раз при обращении
    // Логика: если RequestId НЕ пустой и НЕ null → true, иначе → false
    // Используется в HTML: @if (Model.ShowRequestId) { показать ID }
}
}
