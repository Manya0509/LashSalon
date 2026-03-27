using Microsoft.AspNetCore.Mvc;
using LashBooking.Domain.Interfaces;
using LashBooking.Domain.Constants;

namespace LashBooking.Web.MVC.Controllers
{
    public class CalendarController : BaseController
    {
        // Убрали: IRepository<Appointment>, IRepository<BlockedSlot>
        // Убрали: StartHour, EndHour
        // Всё это теперь внутри ScheduleService.
        private readonly IScheduleService _scheduleService;

        public CalendarController(
            IScheduleService scheduleService,
            ILogger logger) : base(logger)
        {
            _scheduleService = scheduleService;
        }

        // GET: /Calendar?year=2026&month=3
        // Показывает календарь на месяц.
        // Каждый день раскрашен: free/partial/full/past/weekend.
        public async Task<IActionResult> Index(int? year, int? month)
        {
            try
            {
                InitRequestInfo();

                var today = DateTime.Today;
                var currentMonth = new DateTime(
                    year ?? today.Year,
                    month ?? today.Month, 1);

                var weeks = new List<List<object>>();

                // Сдвиг для начала недели с понедельника
                // (стандартно DayOfWeek.Sunday = 0, нам нужен Monday = 0)
                int shift = ((int)currentMonth.DayOfWeek + 6) % 7;

                // Собираем 42 дня (6 недель по 7 дней) — так заполняется сетка календаря
                var allDays = new List<DateTime>();
                var dayStates = new Dictionary<string, string>();

                for (int i = -shift; i < 42 - shift; i++)
                {
                    allDays.Add(currentMonth.AddDays(i));
                }

                // ===== БЫЛО: 40 строк с циклами, блокировками, подсчётом слотов =====
                // ===== СТАЛО: для каждого дня спрашиваем сервис =====
                foreach (var d in allDays)
                {
                    var dateKey = d.Date.ToString("yyyy-MM-dd");
                    dayStates[dateKey] = await _scheduleService.GetDayStateAsync(d);
                }

                // Формируем недели для отображения в таблице
                for (int i = 0; i < 6; i++)
                {
                    var week = allDays.Skip(i * 7).Take(7)
                        .Select(d => (object)new
                        {
                            day = d.Day,
                            date = d.ToString("yyyy-MM-dd"),
                            state = dayStates[d.ToString("yyyy-MM-dd")],
                            isPast = d.Date < today,
                            isWeekend = WorkSchedule.IsWeekend(d),
                            isCurrentMonth = d.Month == currentMonth.Month
                        })
                        .ToList();
                    weeks.Add(week);
                }

                // Навигация: стрелки "предыдущий/следующий месяц"
                ViewBag.Weeks = weeks;
                ViewBag.MonthLabel = currentMonth.ToString("MMMM yyyy");
                ViewBag.PrevYear = currentMonth.AddMonths(-1).Year;
                ViewBag.PrevMonth = currentMonth.AddMonths(-1).Month;
                ViewBag.NextYear = currentMonth.AddMonths(1).Year;
                ViewBag.NextMonth = currentMonth.AddMonths(1).Month;

                return View();
            }
            catch (Exception ex)
            {
                CatchException(ex, "CalendarController/Index", ErrorLevel.Error);
                return RedirectToAction("Index", "Home");
            }
        }

        // GET: /Calendar/Slots?date=2026-03-15
        // AJAX-запрос — при клике на день возвращает свободные часы.
        //
        // ===== БЫЛО: 30 строк с блокировками, записями, циклами =====
        // ===== СТАЛО: одна строка =====
        public async Task<IActionResult> Slots(string date)
        {
            try
            {
                if (!DateTime.TryParse(date, out DateTime selectedDate))
                    return Json(new List<string>());

                var freeSlots = await _scheduleService
                    .GetFreeSlotsForDateAsync(selectedDate);

                return Json(freeSlots);
            }
            catch (Exception ex)
            {
                CatchException(ex, "CalendarController/Slots", ErrorLevel.Warning);
                return Json(new List<string>());
            }
        }
    }
}
