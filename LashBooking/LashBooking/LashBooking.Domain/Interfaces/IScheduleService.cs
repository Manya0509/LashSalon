using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LashBooking.Domain.Models;

namespace LashBooking.Domain.Interfaces
{
    // Контракт сервиса расписания.
    public interface IScheduleService
    {
        // Получить список слотов на конкретную дату для конкретной услуги.
        Task<List<SlotInfo>> GetSlotsAsync(DateTime date, int serviceId);

        // Получить список дат, на которые можно записаться.
        Task<List<DateTime>> GetAvailableDatesAsync(int serviceId);

        // Получить список свободных часов для конкретной даты 
        Task<List<string>> GetFreeSlotsForDateAsync(DateTime date);

        // Получить состояние дня: "free", "partial", "full", "past", "weekend".
        Task<string> GetDayStateAsync(DateTime date);
    }
}

