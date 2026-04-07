using LashBooking.Domain.Constants;
using LashBooking.Domain.Entities;
using LashBooking.Domain.Interfaces;
using LashBooking.Domain.Models;

namespace LashBooking.Web.MVC.Services
{
    // Реализация IScheduleService.
    // Содержит всю логику работы с расписанием:
    public class ScheduleService : IScheduleService
    {
        // Репозитории для доступа к базе данных.
        private readonly IRepository<Service> _serviceRepo;
        private readonly IRepository<Appointment> _appointmentRepo;
        private readonly IRepository<BlockedSlot> _blockedSlotRepo;

        // Конструктор — ASP.NET Core сам подставит репозитории через DI.
        // Так же как раньше подставлял их в контроллеры.
        public ScheduleService(
            IRepository<Service> serviceRepo,
            IRepository<Appointment> appointmentRepo,
            IRepository<BlockedSlot> blockedSlotRepo)
        {
            _serviceRepo = serviceRepo;
            _appointmentRepo = appointmentRepo;
            _blockedSlotRepo = blockedSlotRepo;
        }

        // =====================================================
        // Получить слоты на конкретную дату для конкретной услуги.
        public async Task<List<SlotInfo>> GetSlotsAsync(DateTime date, int serviceId)
        {
            var slots = new List<SlotInfo>();

            // Ищем активную услугу по Id
            var service = (await _serviceRepo.GetAllAsync())
                .FirstOrDefault(s => s.Id == serviceId && s.IsActive);
            if (service == null) return slots;

            // Выходные — слотов нет
            if (WorkSchedule.IsWeekend(date)) return slots;

            // Получаем все блокировки на этот день
            var blockedSlots = (await _blockedSlotRepo
                .FindAsync(b => b.Date.Date == date.Date)).ToList();

            // Если весь день заблокирован (BlockedHour == null означает весь день)
            if (blockedSlots.Any(b => b.BlockedHour == null))
                return slots;

            // Собираем заблокированные часы в HashSet для быстрой проверки
            // HashSet — как список, но проверка "содержит ли?" работает мгновенно
            var blockedHours = blockedSlots
                .Where(b => b.BlockedHour != null)
                .Select(b => b.BlockedHour!.Value)
                .ToHashSet();

            // Конец рабочего дня (18:00)
            var workDayEnd = date.Date.AddHours(WorkSchedule.EndHour);

            // Все записи на этот день (чтобы проверить пересечения)
            var appointments = (await _appointmentRepo
                .FindAsync(a => a.DateStart.Date == date.Date)).ToList();

            // Перебираем каждый рабочий час: 9, 10, 11, ... 17
            for (int h = WorkSchedule.StartHour; h < WorkSchedule.EndHour; h++)
            {
                var slotTime = date.Date.AddHours(h);          // Начало слота (напр. 10:00)
                var slotEnd = slotTime.AddMinutes(service.DurationMinutes); // Конец (напр. 11:30)


                if (slotEnd > workDayEnd) continue;

                // Проверяем: час заблокирован ИЛИ пересекается с другой записью?
                bool conflict = blockedHours.Contains(h) ||
                    appointments.Any(a => slotTime < a.DateEnd && slotEnd > a.DateStart);

                slots.Add(new SlotInfo { Time = slotTime, IsBusy = conflict });
            }

            return slots;
        }

        // Получить даты, на которые можно записаться (ближайшие 30 дней).
        public async Task<List<DateTime>> GetAvailableDatesAsync(int serviceId)
        {
            var available = new List<DateTime>();

            // Находим услугу
            var service = (await _serviceRepo.GetAllAsync())
                .FirstOrDefault(s => s.Id == serviceId && s.IsActive);
            if (service == null) return available;

            // Загружаем все блокировки на ближайшие 30 дней одним запросом
            var allBlocked = (await _blockedSlotRepo.GetAllAsync())
                .Where(b => b.Date >= DateTime.Today
                    && b.Date <= DateTime.Today.AddDays(WorkSchedule.BookingDays))
                .ToList();

            // Перебираем каждый день
            for (int i = 0; i < WorkSchedule.BookingDays; i++)
            {
                var date = DateTime.Today.AddDays(i);

                // Выходные — пропускаем
                if (WorkSchedule.IsWeekend(date)) continue;

                // Весь день заблокирован — пропускаем
                if (allBlocked.Any(b => b.Date.Date == date && b.BlockedHour == null))
                    continue;

                // Заблокированные часы этого дня
                var blockedHours = allBlocked
                    .Where(b => b.Date.Date == date && b.BlockedHour != null)
                    .Select(b => b.BlockedHour!.Value)
                    .ToHashSet();

                var workDayEnd = date.Date.AddHours(WorkSchedule.EndHour);

                // Записи на этот день
                var appointments = (await _appointmentRepo
                    .FindAsync(a => a.DateStart.Date == date.Date)).ToList();

                // Ищем хотя бы один свободный слот
                for (int h = WorkSchedule.StartHour; h < WorkSchedule.EndHour; h++)
                {
                    var slotTime = date.Date.AddHours(h);
                    var slotEnd = slotTime.AddMinutes(service.DurationMinutes);

                    if (slotTime < DateTime.Now) continue;  
                    if (slotEnd > workDayEnd) continue;    
                    if (blockedHours.Contains(h)) continue; 

                    bool conflict = appointments.Any(a =>
                        slotTime < a.DateEnd && slotEnd > a.DateStart);

                    if (!conflict)
                    {
                        available.Add(date);  // Нашли свободный слот — день доступен
                        break;                // Дальше не проверяем — одного достаточно
                    }
                }
            }

            return available;
        }

        // Получить свободные часы на конкретную дату.
        public async Task<List<string>> GetFreeSlotsForDateAsync(DateTime date)
        {
            var freeSlots = new List<string>();

            // Прошедший день или выходной — пусто
            if (date.Date < DateTime.Today) return freeSlots;
            if (WorkSchedule.IsWeekend(date)) return freeSlots;

            var blockedSlots = (await _blockedSlotRepo
                .FindAsync(b => b.Date.Date == date.Date)).ToList();

            // Весь день заблокирован
            if (blockedSlots.Any(b => b.BlockedHour == null))
                return freeSlots;

            var blockedHours = blockedSlots
                .Where(b => b.BlockedHour != null)
                .Select(b => b.BlockedHour!.Value)
                .ToHashSet();

            var appointments = (await _appointmentRepo
                .FindAsync(a => a.DateStart.Date == date.Date)).ToList();

            for (int h = WorkSchedule.StartHour; h < WorkSchedule.EndHour; h++)
            {
                var slot = date.Date.AddHours(h);
                bool busy = blockedHours.Contains(h) ||
                    appointments.Any(a => slot >= a.DateStart && slot < a.DateEnd);

                if (!busy)
                    freeSlots.Add(slot.ToString("HH:mm"));
            }

            return freeSlots;
        }

        // Определить состояние дня для календаря.
        public async Task<string> GetDayStateAsync(DateTime date)
        {
            if (date.Date < DateTime.Today) return "past";
            if (WorkSchedule.IsWeekend(date)) return "weekend";

            var blockedSlots = (await _blockedSlotRepo
                .FindAsync(b => b.Date.Date == date.Date)).ToList();

            // Весь день заблокирован
            if (blockedSlots.Any(b => b.BlockedHour == null))
                return "full";

            var blockedHours = blockedSlots
                .Where(b => b.BlockedHour != null)
                .Select(b => b.BlockedHour!.Value)
                .ToHashSet();

            var appointments = (await _appointmentRepo
                .FindAsync(a => a.DateStart.Date == date.Date)).ToList();

            int totalSlots = WorkSchedule.EndHour - WorkSchedule.StartHour; // 9 слотов
            int busySlots = 0;

            for (int h = WorkSchedule.StartHour; h < WorkSchedule.EndHour; h++)
            {
                var slotStart = date.Date.AddHours(h);
                var slotEnd = slotStart.AddHours(1);
                bool isBusy = blockedHours.Contains(h) ||
                    appointments.Any(a => slotStart < a.DateEnd && slotEnd > a.DateStart);
                if (isBusy) busySlots++;
            }

            if (busySlots == 0) return "free";
            if (busySlots >= totalSlots) return "full";
            return "partial";
        }
    }
}
