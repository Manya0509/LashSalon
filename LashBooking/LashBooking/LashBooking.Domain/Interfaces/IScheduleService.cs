using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LashBooking.Domain.Models;

namespace LashBooking.Domain.Interfaces
{
    // Контракт сервиса расписания.
    // Отвечает за всё что связано со слотами, доступностью и календарём.
    // Используется в BookingController и CalendarController —
    // вместо дублирования логики в каждом из них.
    public interface IScheduleService
    {
        // Получить список слотов на конкретную дату для конкретной услуги.
        // Каждый слот — час (9:00, 10:00, ...), с пометкой занят/свободен.
        // Используется на странице бронирования для отображения сетки времени.
        Task<List<SlotInfo>> GetSlotsAsync(DateTime date, int serviceId);

        // Получить список дат, на которые можно записаться.
        // Исключает выходные, заблокированные дни, и дни где все часы заняты.
        // Используется для подсветки доступных дней в датапикере.
        Task<List<DateTime>> GetAvailableDatesAsync(int serviceId);

        // Получить список свободных часов для конкретной даты (["09:00", "11:00", ...]).
        // Не привязан к конкретной услуге — просто свободные часы.
        // Используется в CalendarController для показа свободного времени.
        Task<List<string>> GetFreeSlotsForDateAsync(DateTime date);

        // Получить состояние дня: "free", "partial", "full", "past", "weekend".
        // Используется в CalendarController для раскраски дней в календаре.
        Task<string> GetDayStateAsync(DateTime date);
    }
}

