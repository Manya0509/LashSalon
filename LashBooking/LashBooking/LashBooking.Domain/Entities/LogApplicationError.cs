using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System;

namespace LashBooking.Domain.Entities
{
    [Table("LogApplicationError")]                  // таблица в БД будет называться "LogApplicationError"
    public class LogApplicationError
    {
        [Key]
        public int LogApplicationErrorId { get; set; } // первичный ключ — уникальный ID записи лога

        public string ErrorContext { get; set; }    // контекст ошибки — где именно произошла (контроллер, метод)
        public string ErrorMsg { get; set; }        // текст сообщения об ошибке
        public DateTime InsertDate { get; set; }    // дата и время когда ошибка была записана
        public string UserData { get; set; }        // данные пользователя в момент ошибки (например ID или имя)
        public int? ErrorLevel { get; set; }        // уровень ошибки (например 1-Info, 2-Warning, 3-Error), ? — может быть null
        public string BrowserInfo { get; set; }     // информация о браузере пользователя (User-Agent)
        public string AppVersion { get; set; }      // версия приложения в момент ошибки
    }
}
