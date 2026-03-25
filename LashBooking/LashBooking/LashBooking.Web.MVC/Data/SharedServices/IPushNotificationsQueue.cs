using LashBooking.Web.MVC.Data.Models;

namespace LashBooking.Web.MVC.Data.SharedServices
{
    public interface IPushNotificationsQueue // интерфейс очереди сообщений
    {
        void Enqueue(LogMessageEntry message); // добавляет сообщение в очередь

        Task<LogMessageEntry> DequeueAsync(CancellationToken cancellationToken); // асинхронно извлекает сообщение из очереди
    }
}