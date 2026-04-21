using LashBooking.Web.MVC.Data.Models;
using System.Reflection;

namespace LashBooking.Web.MVC.Data.SharedServices
{
    public class Logger : ILogger // реализация стандартного интерфейса ILogger
    {
        private readonly object _lock = new object();
        private IPushNotificationsQueue Pr { get; }
        private readonly IHttpContextAccessor _httpContextAccessor;
        public Logger(IPushNotificationsQueue pr, IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
            Pr = pr;
        }
        public IDisposable BeginScope<TState>(TState state)
        {
            return null;
        }

        public bool IsEnabled(LogLevel logLevel) // логируем только Warning и выше
        {
            return logLevel > LogLevel.Warning;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter) // парсит состояние по символу * (формат UserData*ErrorMsg*BrowserInfo), создаёт LogMessageEntry и кладёт его в очередь через Pr.Enqueue(). Также выводит в консоль через Console.WriteLine().
        {
            if (formatter != null && IsEnabled(logLevel))
            {
                lock (_lock)
                {
                    var login_state = state.ToString().Split('*');
                    LogMessageEntry message = new LogMessageEntry()
                    {
                        ErrorMsg = (login_state.Length > 1 ? login_state[1] + Environment.NewLine : "") + exception?.Message + exception?.InnerException?.Message,
                        UserData = login_state.Length > 1 ? login_state[0] : "Unknown",
                        ErrorLevel = (int)logLevel,
                        ErrorContext = exception?.StackTrace ?? "",
                        BrowserInfo = login_state.Length > 2 ? login_state[2] : "",
                        AppVersion = GetAppTitle(),
                        InsertDate = DateTime.UtcNow
                    };
                    if (string.IsNullOrEmpty(message.ErrorMsg)) message.ErrorMsg = formatter(state, exception);
                    Pr.Enqueue(message);
                    Console.WriteLine(formatter(state, exception) + Environment.NewLine);
                }
            }
        }

        private static string GetAppTitle()
        {
            string name = Assembly.GetExecutingAssembly().FullName;
            AssemblyName asmName = new AssemblyName(name);
            string s = AppName() + " " + asmName.Version.Major + "." + asmName.Version.Minor + "." + asmName.Version.Build;
            //var version = typeof(Program).Assembly.GetName().Version;
            //string s = AppName() + " " + version.Major + "." + version.Minor + "." + version.Build;
            return s;
        }
        public static string AppName()
        {
            return "LashSalon";
        }
    }
}
