using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LashBooking.Domain.Constants
{
    // Настройки рабочего графика мастера.
    // Все контроллеры и сервисы берут часы работы отсюда,
    // чтобы не дублировать значения в разных местах.
    public static class WorkSchedule
    {
        // Начало рабочего дня (9:00)
        public const int StartHour = 9;

        // Конец рабочего дня (18:00)
        public const int EndHour = 18;

        // На сколько дней вперёд можно записаться
        public const int BookingDays = 30;

        // Проверяет, является ли день выходным (суббота или воскресенье)
        // Используется в календаре и бронировании
        public static bool IsWeekend(DateTime date) =>
            date.DayOfWeek == DayOfWeek.Saturday ||
            date.DayOfWeek == DayOfWeek.Sunday;

        // Проверяет, попадает ли час в рабочее время
        public static bool IsWithinWorkingHours(int hour) =>
            hour >= StartHour && hour < EndHour;
    }
}

