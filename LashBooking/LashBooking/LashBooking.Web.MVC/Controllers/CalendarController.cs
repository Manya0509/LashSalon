using Microsoft.AspNetCore.Mvc;
using LashBooking.Domain.Interfaces;
using LashBooking.Domain.Entities;

namespace LashBooking.Web.MVC.Controllers
{
    public class CalendarController : Controller
    {
        private readonly IRepository<Appointment> _appointmentRepo;
        private const int StartHour = 9;
        private const int EndHour = 18;

        public CalendarController(IRepository<Appointment> appointmentRepo)
        {
            _appointmentRepo = appointmentRepo;
        }

        // GET: /Calendar?year=2026&month=3
        public async Task<IActionResult> Index(int? year, int? month)
        {
            var today = DateTime.Today;
            var currentMonth = new DateTime(year ?? today.Year, month ?? today.Month, 1);
            var weeks = new List<List<object>>();
            var dayStates = new Dictionary<string, string>();

            int shift = ((int)currentMonth.DayOfWeek + 6) % 7;
            var allDays = new List<DateTime>();

            for (int i = -shift; i < 42 - shift; i++)
            {
                var d = currentMonth.AddDays(i);
                var dateKey = d.Date.ToString("yyyy-MM-dd");
                allDays.Add(d);

                // Прошедшие дни
                if (d.Date < today)
                {
                    dayStates[dateKey] = "past";
                    continue;
                }

                // Выходные — суббота (6) и воскресенье (0)
                if (d.DayOfWeek == DayOfWeek.Saturday || d.DayOfWeek == DayOfWeek.Sunday)
                {
                    dayStates[dateKey] = "weekend";
                    continue;
                }

                // Рабочие дни — считаем занятость
                var appts = (await _appointmentRepo.FindAsync(a => a.DateStart.Date == d.Date)).ToList();
                int totalSlots = EndHour - StartHour;
                int busy = appts.Count;

                if (busy == 0)
                    dayStates[dateKey] = "free";
                else if (busy >= totalSlots)
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

            // Выходные — слотов нет
            if (selectedDate.DayOfWeek == DayOfWeek.Saturday || selectedDate.DayOfWeek == DayOfWeek.Sunday)
                return Json(new List<string>());

            var appts = (await _appointmentRepo.FindAsync(a => a.DateStart.Date == selectedDate.Date)).ToList();
            var freeSlots = new List<string>();

            for (int h = StartHour; h < EndHour; h++)
            {
                var slot = selectedDate.Date.AddHours(h);
                bool busy = appts.Any(a => slot >= a.DateStart && slot < a.DateEnd);
                if (!busy)
                    freeSlots.Add(slot.ToString("HH:mm"));
            }

            return Json(freeSlots);
        }
    }
}
