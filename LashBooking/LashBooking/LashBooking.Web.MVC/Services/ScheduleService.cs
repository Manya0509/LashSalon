using LashBooking.Domain.Constants;
using LashBooking.Domain.Entities;
using LashBooking.Domain.Interfaces;
using LashBooking.Domain.Models;

namespace LashBooking.Web.MVC.Services
{
    // Реализация IScheduleService.
    // Содержит ВСЮ логику работы с расписанием:
    // слоты, доступные даты, состояния дней.
    // Раньше эта логика была раскидана по BookingController и CalendarController.
    public class ScheduleService : IScheduleService
    {
        // Репозитории для доступа к базе данных.
        // Сервис не ходит в базу напрямую — он просит репозитории.
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
        // Возвращает список: [{Time: 9:00, IsBusy: false}, {Time: 10:00, IsBusy: true}, ...]
        //
        // Логика:
        // 1. Находим услугу (чтобы знать длительность)
        // 2. Проверяем: не выходной ли день?
        // 3. Проверяем: не заблокирован ли весь день?
        // 4. Для каждого часа (9, 10, 11, ... 17):
        //    — хватит ли времени до конца рабочего дня?
        //    — не заблокирован ли этот час?
        //    — нет ли пересечения с другими записями?
        // =====================================================
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
                // Пересечение: слот 10:00-11:30 пересекается с записью 10:30-11:30
                // Формула: slotTime < a.DateEnd && slotEnd > a.DateStart
                bool conflict = blockedHours.Contains(h) ||
                    appointments.Any(a => slotTime < a.DateEnd && slotEnd > a.DateStart);

                slots.Add(new SlotInfo { Time = slotTime, IsBusy = conflict });
            }

            return slots;
        }

        // =====================================================
        // Получить даты, на которые можно записаться (ближайшие 30 дней).
        // Дата считается доступной, если на ней есть хотя бы один свободный слот.
        //
        // Используется для подсветки дней в календаре на странице бронирования:
        // зелёный = есть свободное время, серый = всё занято.
        // =====================================================
        public async Task<List<DateTime>> GetAvailableDatesAsync(int serviceId)
        {
            var available = new List<DateTime>();

            // Находим услугу
            var service = (await _serviceRepo.GetAllAsync())
                .FirstOrDefault(s => s.Id == serviceId && s.IsActive);
            if (service == null) return available;

            // Загружаем все блокировки на ближайшие 30 дней одним запросом
            // (а не по одному запросу на каждый день — так быстрее)
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

                    if (slotTime < DateTime.Now) continue;   // Прошедшее время — пропускаем
                    if (slotEnd > workDayEnd) continue;       // Не влезает — пропускаем
                    if (blockedHours.Contains(h)) continue;   // Заблокирован — пропускаем

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

        // =====================================================
        // Получить свободные часы на конкретную дату.
        // Возвращает строки: ["09:00", "11:00", "14:00"]
        //
        // Используется в CalendarController — при клике на день
        // показывает свободные часы.
        // Не привязан к услуге — просто свободен ли час.
        // =====================================================
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

        // =====================================================
        // Определить состояние дня для календаря.
        // Возвращает строку:
        //   "past"    — день уже прошёл (серый)
        //   "weekend" — выходной (серый)
        //   "full"    — всё занято или заблокировано (красный)
        //   "partial" — часть слотов занята (жёлтый)
        //   "free"    — всё свободно (зелёный)
        //
        // Используется в CalendarController для раскраски календаря.
        // =====================================================
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
