using LashBooking.Domain.Entities;
using LashBooking.Infrastructure.Data;
using LashBooking.Web.MVC.Data.Models;
using System.Reflection;

namespace LashBooking.Web.MVC.Data.SharedServices
{
    public class LogApplicationErrorService : ILogService // реализация ILogService
    {
        public void CreateLogRecord(LogMessageEntry message, IServiceScope scope) // получает контекст базы данных через scope, создаёт запись LogApplicationError и сохраняет через context.Insert()
        {
            var _context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var appRepo = _context.Insert(new LogApplicationError()
            {
                InsertDate = DateTime.Now,
                ErrorMsg = message.ErrorMsg,
                ErrorLevel = message.ErrorLevel,
                UserData = message.UserData,
                ErrorContext = message.ErrorContext,
                BrowserInfo = message.BrowserInfo,
                AppVersion = GetAppTitle()
            });
        }
        private static string GetAppTitle() // читает версию сборки через Reflection и возвращает строку вида LashSalon 1.0.0.
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
