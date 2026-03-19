using Microsoft.AspNetCore.Mvc;
using LashBooking.Domain.Interfaces;
using LashBooking.Domain.Entities;

namespace LashBooking.Web.MVC.Controllers
{
    public class CalendarController : Controller
    {
        private readonly IRepository<Appointment> _appointmentRepo;
        private readonly IRepository<BlockedSlot> _blockedSlotRepo;
        private const int StartHour = 9;
        private const int EndHour = 18;

        public CalendarController(
            IRepository<Appointment> appointmentRepo,
            IRepository<BlockedSlot> blockedSlotRepo)
        {
            _appointmentRepo = appointmentRepo;
            _blockedSlotRepo = blockedSlotRepo;
        }

        // GET: /Calendar?year=2026&month=3
        public async Task<IActionResult> Index(int? year, int? month)
        {
            var today = DateTime.Today;
            var currentMonth = new DateTime(year ?? today.Year, month ?? today.Month, 1);
            var weeks = new List<List<object>>();
            var dayStates = new Dictionary<string, string>();

            // Загружаем все блокировки за этот месяц
            var blockedSlots = (await _blockedSlotRepo.GetAllAsync())
                .Where(b => b.Date.Year == currentMonth.Year && b.Date.Month == currentMonth.Month)
                .ToList();

            int shift = ((int)currentMonth.DayOfWeek + 6) % 7;
            var allDays = new List<DateTime>();

            for (int i = -shift; i < 42 - shift; i++)
            {
                var d = currentMonth.AddDays(i);
                var dateKey = d.Date.ToString("yyyy-MM-dd");
                allDays.Add(d);

                if (d.Date < today)
                {
                    dayStates[dateKey] = "past";
                    continue;
                }

                if (d.DayOfWeek == DayOfWeek.Saturday || d.DayOfWeek == DayOfWeek.Sunday)
                {
                    dayStates[dateKey] = "weekend";
                    continue;
                }

                // Проверяем — заблокирован ли весь день
                bool dayBlocked = blockedSlots.Any(b => b.Date.Date == d.Date && b.BlockedHour == null);
                if (dayBlocked)
                {
                    dayStates[dateKey] = "full";
                    continue;
                }

                // Считаем занятость с учётом блокировок по часам
                var appts = (await _appointmentRepo.FindAsync(a => a.DateStart.Date == d.Date)).ToList();
                var blockedHours = blockedSlots.Where(b => b.Date.Date == d.Date && b.BlockedHour != null).Select(b => b.BlockedHour!.Value).ToHashSet();
                int totalSlots = EndHour - StartHour;
                int busySlots = 0;

                for (int h = StartHour; h < EndHour; h++)
                {
                    var slotStart = d.Date.AddHours(h);
                    var slotEnd = slotStart.AddHours(1);

                    bool isBusy = blockedHours.Contains(h) ||
                                  appts.Any(a => slotStart < a.DateEnd && slotEnd > a.DateStart);
                    if (isBusy) busySlots++;
                }

                if (busySlots == 0)
                    dayStates[dateKey] = "free";
                else if (busySlots >= totalSlots)
                    dayStates[dateKey] = "full";
                else
                    dayStates[dateKey] = "partial";
            }

            for (int i = 0; i < 6; i++)
            {
                var week = allDays.Skip(i * 7).Take(7)
                    .Select(d => (object)new
                    {
                        day = d.Day,
                        date = d.ToString("yyyy-MM-dd"),
                        state = dayStates.ContainsKey(d.ToString("yyyy-MM-dd"))
                                            ? dayStates[d.ToString("yyyy-MM-dd")]
                                            : "",
                        isPast = d.Date < today,
                        isWeekend = d.DayOfWeek == DayOfWeek.Saturday || d.DayOfWeek == DayOfWeek.Sunday,
                        isCurrentMonth = d.Month == currentMonth.Month
                    })
                    .ToList();
                weeks.Add(week);
            }

            ViewBag.Weeks = weeks;
            ViewBag.MonthLabel = currentMonth.ToString("MMMM yyyy");
            ViewBag.PrevYear = currentMonth.AddMonths(-1).Year;
            ViewBag.PrevMonth = currentMonth.AddMonths(-1).Month;
            ViewBag.NextYear = currentMonth.AddMonths(1).Year;
            ViewBag.NextMonth = currentMonth.AddMonths(1).Month;

            return View();
        }

        // GET: /Calendar/Slots?date=2026-03-15
        public async Task<IActionResult> Slots(string date)
        {
            if (!DateTime.TryParse(date, out DateTime selectedDate))
                return Json(new List<string>());

            if (selectedDate.Date < DateTime.Today)
                return Json(new List<string>());

            if (selectedDate.DayOfWeek == DayOfWeek.Saturday || selectedDate.DayOfWeek == DayOfWeek.Sunday)
                return Json(new List<string>());

            // Проверяем — заблокирован ли весь день
            var blockedSlots = (await _blockedSlotRepo.FindAsync(b => b.Date.Date == selectedDate.Date)).ToList();
            bool dayBlocked = blockedSlots.Any(b => b.BlockedHour == null);
            if (dayBlocked)
                return Json(new List<string>());

            var blockedHours = blockedSlots.Where(b => b.BlockedHour != null).Select(b => b.BlockedHour!.Value).ToHashSet();
            var appts = (await _appointmentRepo.FindAsync(a => a.DateStart.Date == selectedDate.Date)).ToList();
            var freeSlots = new List<string>();

            for (int h = StartHour; h < EndHour; h++)
            {
                var slot = selectedDate.Date.AddHours(h);
                bool busy = blockedHours.Contains(h) ||
                            appts.Any(a => slot >= a.DateStart && slot < a.DateEnd);
                if (!busy)
                    freeSlots.Add(slot.ToString("HH:mm"));
            }

            return Json(freeSlots);
        }
    }
}
