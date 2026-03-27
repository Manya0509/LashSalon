using Microsoft.AspNetCore.Mvc;
using LashBooking.Domain.Interfaces;
using LashBooking.Domain.Entities;
using LashBooking.Domain.Constants;
using LashBooking.Web.MVC.Models;

namespace LashBooking.Web.MVC.Controllers
{
    public class BookingController : BaseController
    {
        // Репозитории — только те, что нужны контроллеру напрямую.
        // Для получения списка услуг и данных клиента.
        private readonly IRepository<Service> _serviceRepo;
        private readonly IRepository<Client> _clientRepo;

        // Сервисы — вся бизнес-логика теперь здесь.
        // Контроллер не считает слоты сам — просит сервис.
        private readonly IScheduleService _scheduleService;
        private readonly IBookingService _bookingService;

        // Убрали: IRepository<Appointment>, IRepository<BlockedSlot>
        // — они больше не нужны контроллеру, ими пользуются сервисы.
        //
        // Убрали: StartHour, EndHour, BookingDays
        // — они теперь в WorkSchedule.

        public BookingController(
            IRepository<Service> serviceRepo,
            IRepository<Client> clientRepo,
            IScheduleService scheduleService,
            IBookingService bookingService,
            ILogger logger) : base(logger)
        {
            _serviceRepo = serviceRepo;
            _clientRepo = clientRepo;
            _scheduleService = scheduleService;
            _bookingService = bookingService;
        }

        // GET: /Booking
        // Страница бронирования — показывает услуги, календарь, слоты.
        public async Task<IActionResult> Index(string? date)
        {
            try
            {
                InitRequestInfo();

                // Получаем активные услуги для выпадающего списка
                var services = (await _serviceRepo.GetAllAsync())
                    .Where(s => s.IsActive).ToList();
                var selectedServiceId = services.FirstOrDefault()?.Id ?? 0;

                // Проверяем авторизован ли клиент, чтобы подставить имя и телефон
                var clientId = HttpContext.Session.GetInt32("ClientId");
                var isAuthenticated = clientId.HasValue;
                string clientName = "";
                string clientPhone = "";

                if (isAuthenticated)
                {
                    var clients = await _clientRepo
                        .FindAsync(c => c.Id == clientId!.Value);
                    var client = clients.FirstOrDefault();
                    if (client != null)
                    {
                        clientName = client.Name ?? "";
                        clientPhone = client.Phone ?? "";
                    }
                }

                // Парсим выбранную дату из URL (если есть)
                DateTime selectedDate = DateTime.Today;
                DateTime selectedTime = default;

                if (!string.IsNullOrEmpty(date)
                    && DateTime.TryParse(date, out DateTime parsedDate))
                {
                    selectedDate = parsedDate.Date;
                    selectedTime = parsedDate;
                }

                // ===== БЫЛО: 40 строк приватного метода GetSlots() =====
                // ===== СТАЛО: одна строка — сервис всё сделает сам =====
                var slots = await _scheduleService
                    .GetSlotsAsync(selectedDate, selectedServiceId);

                // Выбираем первый свободный слот по умолчанию
                if (selectedTime == default)
                {
                    selectedTime = slots
                        .Where(s => !s.IsBusy && s.Time >= DateTime.Now)
                        .OrderBy(s => s.Time)
                        .FirstOrDefault()?.Time ?? default;
                }

                // ===== БЫЛО: 50 строк приватного метода GetAvailableDates() =====
                // ===== СТАЛО: одна строка =====
                var availableDates = await _scheduleService
                    .GetAvailableDatesAsync(selectedServiceId);

                // Передаём данные во View через ViewBag
                ViewBag.Services = services;
                ViewBag.SelectedServiceId = selectedServiceId;
                ViewBag.ClientName = clientName;
                ViewBag.ClientPhone = clientPhone;
                ViewBag.IsAuthenticated = isAuthenticated;
                ViewBag.SelectedDate = selectedDate.ToString("yyyy-MM-dd");
                ViewBag.SelectedDateDisplay = selectedDate.ToString("dd.MM.yyyy");
                ViewBag.SelectedTime = selectedTime != default
                    ? selectedTime.ToString("yyyy-MM-ddTHH:mm:ss") : "";
                ViewBag.Slots = slots;
                ViewBag.AvailableDates = availableDates
                    .Select(d => d.ToString("yyyy-MM-dd")).ToList();
                ViewBag.Message = TempData["Message"];
                ViewBag.IsSuccess = TempData["IsSuccess"];

                return View();
            }
            catch (Exception ex)
            {
                CatchException(ex, "BookingController/Index", ErrorLevel.Error);
                return RedirectToAction("Index", "Home");
            }
        }

        // GET: /Booking/GetSlotsByDate?date=2026-03-15&serviceId=1
        // AJAX-запрос — возвращает слоты в формате JSON.
        // Вызывается когда клиент выбирает другую дату в календаре.
        public async Task<IActionResult> GetSlotsByDate(string date, int serviceId)
        {
            try
            {
                if (!DateTime.TryParse(date, out DateTime selectedDate))
                    return Json(new List<object>());

                // Одна строка вместо дублирования логики
                var slots = await _scheduleService
                    .GetSlotsAsync(selectedDate, serviceId);

                return Json(slots.Select(s => new
                {
                    time = s.Time.ToString("yyyy-MM-ddTHH:mm:ss"),
                    timeDisplay = s.Time.ToString("HH:mm"),
                    isBusy = s.IsBusy
                }));
            }
            catch (Exception ex)
            {
                CatchException(ex, "BookingController/GetSlotsByDate",
                    ErrorLevel.Warning);
                return Json(new List<object>());
            }
        }

        // GET: /Booking/GetAvailableDatesAction?serviceId=1
        // AJAX-запрос — возвращает доступные даты для подсветки в календаре.
        public async Task<IActionResult> GetAvailableDatesAction(int serviceId)
        {
            try
            {
                // Одна строка вместо дублирования логики
                var dates = await _scheduleService
                    .GetAvailableDatesAsync(serviceId);
                return Json(dates.Select(d => d.ToString("yyyy-MM-dd")));
            }
            catch (Exception ex)
            {
                CatchException(ex, "BookingController/GetAvailableDatesAction",
                    ErrorLevel.Warning);
                return Json(new List<string>());
            }
        }

        // POST: /Booking/Save
        // Создание записи.
        //
        // ===== БЫЛО: 80 строк проверок и создания =====
        // ===== СТАЛО: валидация формы + вызов сервиса =====
        //
        // Контроллер отвечает только за:
        // 1. Проверку ModelState (валидация формы)
        // 2. Передачу данных сервису
        // 3. Показ результата пользователю (TempData)
        //
        // Контроллер НЕ отвечает за:
        // — проверку блокировок (делает BookingService)
        // — проверку дубликатов (делает BookingService)
        // — создание клиента (делает BookingService)
        // — создание записи (делает BookingService)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Save(BookingViewModel model)
        {
            try
            {
                InitRequestInfo();

                // Валидация формы — это ответственность контроллера
                if (!ModelState.IsValid)
                {
                    TempData["Message"] = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .FirstOrDefault() ?? "Пожалуйста, заполните все поля";
                    TempData["IsSuccess"] = false;
                    return RedirectToAction("Index");
                }

                // Получаем Id клиента из сессии (null если не авторизован)
                var clientId = HttpContext.Session.GetInt32("ClientId");

                // Вся логика — в сервисе. Одна строка.
                var result = await _bookingService.CreateBookingAsync(
                    model.ServiceId,
                    model.SelectedTime,
                    model.ClientName,
                    model.ClientPhone,
                    clientId);

                // Показываем результат пользователю
                TempData["Message"] = result.Message;
                TempData["IsSuccess"] = result.Success;
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                CatchException(ex, "BookingController/Save", ErrorLevel.Error);
                TempData["Message"] = "Не удалось создать запись.";
                TempData["IsSuccess"] = false;
                return RedirectToAction("Index");
            }
        }

        // Убрали:
        // — private async Task<List<Slot>> GetSlots(...)     → теперь в ScheduleService
        // — private async Task<List<DateTime>> GetAvailableDates(...) → теперь в ScheduleService
        // — public class Slot { ... }                        → теперь SlotInfo в Domain
    }
}
