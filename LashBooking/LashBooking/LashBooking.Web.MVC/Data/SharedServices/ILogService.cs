using LashBooking.Web.MVC.Data.Models;

namespace LashBooking.Web.MVC.Data.SharedServices
{
    public interface ILogService // интерфейс сервиса логирования. Определяет один метод CreateLogRecord() который принимает сообщение об ошибке и IServiceScope для доступа к базе данных.
    {
        public void CreateLogRecord(LogMessageEntry message, IServiceScope scope);
    }
}
