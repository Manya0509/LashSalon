using LashBooking.Domain.Entities;
using LashBooking.Infrastructure.Data;
using LashBooking.Web.MVC.Data.Models;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;

namespace LashBooking.Web.MVC.Data.SharedServices
{
    public class PushNotificationsDequeuer : IHostedService, IDisposable
    // PushNotificationsDequeuer.cs — фоновый сервис (IHostedService) который каждые 10 секунд проверяет очередь и записывает накопленные ошибки в базу. Работает асинхронно в фоне не блокируя основной поток. Если запись в базу упала — пишет в Windows EventLog как запасной вариант.
    {
        private Timer _timer;
        private Task _executingTask;
        private readonly IPushNotificationsQueue _messagesQueue;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly CancellationTokenSource _stoppingCts = new CancellationTokenSource();

        public PushNotificationsDequeuer(IPushNotificationsQueue messagesQueue, IServiceScopeFactory scopeFactory)
        {
            _messagesQueue = messagesQueue;
            _scopeFactory = scopeFactory;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _timer = new Timer(ExecuteTask, null, TimeSpan.FromSeconds(10), TimeSpan.FromMilliseconds(-1));

            return Task.CompletedTask;
        }

        private void ExecuteTask(object state)
        {
            _timer?.Change(Timeout.Infinite, 0);
            _executingTask = ExecuteTaskAsync(_stoppingCts.Token);
        }

        private async Task DequeueMessagesAsync(CancellationToken stoppingToken)
        {
            LogMessageEntry message;
            do
            {
                message = await _messagesQueue.DequeueAsync(stoppingToken);

                if (!stoppingToken.IsCancellationRequested)
                {
                    try
                    {
                        using (var scope = _scopeFactory.CreateScope())
                        {
                            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                            context.Insert(new LogApplicationError()
                            {
                                InsertDate = DateTime.Now,
                                ErrorMsg = message.ErrorMsg,
                                ErrorLevel = message.ErrorLevel,
                                UserData = message.UserData,
                                ErrorContext = message.ErrorContext,
                                BrowserInfo = message.BrowserInfo,
                                AppVersion = message.AppVersion
                            });
                        }
                    }
                    catch (Exception e)
                    {
                        if (!EventLog.SourceExists(ConstField.LogSource))
                            EventLog.CreateEventSource(ConstField.LogSource, ConstField.LogName);
                        EventLog.WriteEntry(ConstField.LogSource,
                            $"Exception with database - {e.Message} \n Exception from client - {message.ErrorMsg}", EventLogEntryType.Error);
                    }
                }
            } while (message != null);
        }

        private async Task ExecuteTaskAsync(CancellationToken stoppingToken)
        {
            await DequeueMessagesAsync(stoppingToken);
            _timer.Change(TimeSpan.FromSeconds(10), TimeSpan.FromMilliseconds(-1));
        }

        public virtual async Task StopAsync(CancellationToken cancellationToken)
        {
            _timer?.Change(Timeout.Infinite, 0);

            if (_executingTask == null)
            {
                return;
            }

            try
            {
                _stoppingCts.Cancel();
            }
            finally
            {
                await Task.WhenAny(_executingTask, Task.Delay(Timeout.Infinite, cancellationToken));
            }
        }

        public void Dispose()
        {
            _stoppingCts.Cancel();
            _timer?.Dispose();
        }
    }
}