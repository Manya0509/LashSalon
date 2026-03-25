using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LashBooking.Domain.Constants
{
    public static class ErrorLevel
    {
        public const int Debug = 1; // отладочная информация
        public const int Info = 2; // обычные события
        public const int Warning = 3; // предупреждение
        public const int Error = 4; // ошибка
        public const int Critical = 5; // критическая
    }
}
