namespace LashBooking.Web.MVC.Data.SharedServices
{
    public class LoggerProvider : ILoggerProvider // фабрика для создания логгеров
    {
        private IPushNotificationsQueue _pr { get; }
        private readonly IHttpContextAccessor _httpContextAccessor;
        //private AuthenticationStateProvider _state;
        public LoggerProvider(IPushNotificationsQueue pr, IHttpContextAccessor httpContextAccessor)//, AuthenticationStateProvider state)
        {
            _httpContextAccessor = httpContextAccessor;
            _pr = pr;
            //_state = state;
        }
        public ILogger CreateLogger(string categoryName) => new Logger(_pr, _httpContextAccessor);

        public void Dispose()
        {
        }

        // вызывает CreateLogger() когда нужен новый экземпляр логгера. Регистрируется в Program.cs как ILoggerProvider.
    }
}
