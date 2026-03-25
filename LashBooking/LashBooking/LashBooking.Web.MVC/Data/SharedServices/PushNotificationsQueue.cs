using LashBooking.Web.MVC.Data.Models;
using System.Collections.Concurrent;

namespace LashBooking.Web.MVC.Data.SharedServices
{
    public class PushNotificationsQueue : IPushNotificationsQueue
    // реализация очереди на основе ConcurrentQueue<T> — потокобезопасная очередь. SemaphoreSlim используется как сигнал — Release() при добавлении, WaitAsync() при извлечении. Это позволяет DequeueAsync() ждать не занимая поток.
    {
        private readonly ConcurrentQueue<LogMessageEntry> _messages = new ConcurrentQueue<LogMessageEntry>();
        private readonly SemaphoreSlim _messageEnqueuedSignal = new SemaphoreSlim(0);

        public void Enqueue(LogMessageEntry message)
        {
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            _messages.Enqueue(message);

            _messageEnqueuedSignal.Release();
        }

        public async Task<LogMessageEntry> DequeueAsync(CancellationToken cancellationToken)
        {
            await _messageEnqueuedSignal.WaitAsync(cancellationToken);

            _messages.TryDequeue(out LogMessageEntry message);

            return message;
        }
    }
}