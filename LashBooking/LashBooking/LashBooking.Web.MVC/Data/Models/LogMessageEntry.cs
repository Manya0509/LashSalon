using System;

namespace LashBooking.Web.MVC.Data.Models
{
    public class LogMessageEntry // это не сущность базы данных, а промежуточная модель. Используется внутри приложения для передачи данных об ошибке между классами до записи в БД.
    {
        public string ErrorMsg { get; set; }
        public string ErrorContext { get; set; }
        public string ErrorMsgUser { get; set; }
        public DateTime InsertDate { get; set; }
        public string UserData { get; set; }
        public int? ErrorLevel { get; set; }
        public string BrowserInfo { get; set; }
        public string AppVersion { get; set; }
    }
}
