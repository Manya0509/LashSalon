using Microsoft.AspNetCore.Mvc;
using LashBooking.Domain.Interfaces;
using LashBooking.Domain.Entities;
using LashBooking.Web.MVC.Models;

namespace LashBooking.Web.MVC.Controllers
{
    public class BookingController : Controller
    {
        private readonly IRepository<Service> _serviceRepo;
        private readonly IRepository<Client> _clientRepo;
        private readonly IRepository<Appointment> _appointmentRepo;
        private readonly IRepository<BlockedSlot> _blockedSlotRepo;

        private const int StartHour = 9;
        private const int EndHour = 18;
        private const int BookingDays = 30;

        public BookingController(
            IRepository<Service> serviceRepo,
            IRepository<Client> clientRepo,
            IRepository<Appointment> appointmentRepo,
            IRepository<BlockedSlot> blockedSlotRepo)
        {
            _serviceRepo = serviceRepo;
            _clientRepo = clientRepo;
            _appointmentRepo = appointmentRepo;
            _blockedSlotRepo = blockedSlotRepo;
        }

        public async Task<IActionResult> Index(string? date)
        {
            var services = (await _serviceRepo.GetAllAsync()).Where(s => s.IsActive).ToList();
            var selectedServiceId = services.FirstOrDefault()?.Id ?? 0;

            var clientId = HttpContext.Session.GetInt32("ClientId");
            var isAuthenticated = clientId.HasValue;
            string clientName = "";
            string clientPhone = "";

            if (isAuthenticated)
            {
                var clients = await _clientRepo.FindAsync(c => c.Id == clientId!.Value);
                var client = clients.FirstOrDefault();
                if (client != null)
                {
                    clientName = client.Name ?? "";
                    clientPhone = client.Phone ?? "";
                }
            }

            DateTime selectedDate = DateTime.Today;
            DateTime selectedTime = default;

            if (!string.IsNullOrEmpty(date) && DateTime.TryParse(date, out DateTime parsedDate))
            {
                selectedDate = parsedDate.Date;
                selectedTime = parsedDate;
            }

            var slots = await GetSlots(selectedDate, selectedServiceId, services);

            if (selectedTime == default)
            {
                selectedTime = slots
                    .Where(s => !s.IsBusy && s.Time >= DateTime.Now)
                    .OrderBy(s => s.Time)
                    .FirstOrDefault()?.Time ?? default;
            }

            var availableDates = await GetAvailableDates(selectedServiceId, services);

            ViewBag.Services = services;
            ViewBag.SelectedServiceId = selectedServiceId;
            ViewBag.ClientName = clientName;
            ViewBag.ClientPhone = clientPhone;
            ViewBag.IsAuthenticated = isAuthenticated;
            ViewBag.SelectedDate = selectedDate.ToString("yyyy-MM-dd");
            ViewBag.SelectedDateDisplay = selectedDate.ToString("dd.MM.yyyy");
            ViewBag.SelectedTime = selectedTime != default ? selectedTime.ToString("yyyy-MM-ddTHH:mm:ss") : "";
            ViewBag.Slots = slots;
            ViewBag.AvailableDates = availableDates.Select(d => d.ToString("yyyy-MM-dd")).ToList();
            ViewBag.Message = TempData["Message"];
            ViewBag.IsSuccess = TempData["IsSuccess"];

            return View();
        }

        public async Task<IActionResult> GetSlotsByDate(string date, int serviceId)
        {
            if (!DateTime.TryParse(date, out DateTime selectedDate))
                return Json(new List<object>());

            var services = (await _serviceRepo.GetAllAsync()).Where(s => s.IsActive).ToList();
            var slots = await GetSlots(selectedDate, serviceId, services);

            return Json(slots.Select(s => new
            {
                time = s.Time.ToString("yyyy-MM-ddTHH:mm:ss"),
                timeDisplay = s.Time.ToString("HH:mm"),
                isBusy = s.IsBusy
            }));
        }

        public async Task<IActionResult> GetAvailableDatesAction(int serviceId)
        {
            var services = (await _serviceRepo.GetAllAsync()).Where(s => s.IsActive).ToList();
            var dates = await GetAvailableDates(serviceId, services);
            return Json(dates.Select(d => d.ToString("yyyy-MM-dd")));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Save(BookingViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["Message"] = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .FirstOrDefault() ?? "Пожалуйста, заполните все поля";
                TempData["IsSuccess"] = false;
                return RedirectToAction("Index");
            }

            if (!DateTime.TryParse(model.SelectedTime, out DateTime time) || time < DateTime.Now)
            {
                TempData["Message"] = "Пожалуйста, выберите доступное время";
                TempData["IsSuccess"] = false;
                return RedirectToAction("Index");
            }

            var services = (await _serviceRepo.GetAllAsync()).ToList();
            var service = services.FirstOrDefault(x => x.Id == model.ServiceId);
            if (service == null)
            {
                TempData["Message"] = "Услуга не найдена";
                TempData["IsSuccess"] = false;
                return RedirectToAction("Index");
            }

            var dateEnd = time.AddMinutes(service.DurationMinutes);
            var workDayEnd = time.Date.AddHours(EndHour);

            if (dateEnd > workDayEnd)
            {
                TempData["Message"] = $"Недостаточно времени для услуги «{service.Name}». Выберите более раннее время.";
                TempData["IsSuccess"] = false;
                return RedirectToAction("Index");
            }

            // Проверка блокировок
            var blockedSlots = (await _blockedSlotRepo.FindAsync(b => b.Date.Date == time.Date)).ToList();
            if (blockedSlots.Any(b => b.BlockedHour == null))
            {
                TempData["Message"] = "Этот день недоступен для записи.";
                TempData["IsSuccess"] = false;
                return RedirectToAction("Index");
            }
            if (blockedSlots.Any(b => b.BlockedHour == time.Hour))
            {
                TempData["Message"] = "Выбранное время недоступно для записи.";
                TempData["IsSuccess"] = false;
                return RedirectToAction("Index");
            }

            var existing = await _appointmentRepo.FindAsync(a =>
                a.Client != null &&
                a.Client.Phone == model.ClientPhone &&
                a.DateStart.Date == time.Date);

            if (existing.Any())
            {
                TempData["Message"] = "У вас уже есть запись на этот день";
                TempData["IsSuccess"] = false;
                return RedirectToAction("Index");
            }

            Client? client = null;

            var clientId = HttpContext.Session.GetInt32("ClientId");
            if (clientId.HasValue)
            {
                var clients = await _clientRepo.FindAsync(c => c.Id == clientId.Value);
                client = clients.FirstOrDefault();
            }

            if (client == null)
            {
                var clients = await _clientRepo.FindAsync(c => c.Phone == model.ClientPhone);
                client = clients.FirstOrDefault();
            }

            if (client == null)
            {
                client = new Client
                {
                    Name = model.ClientName,
                    Phone = model.ClientPhone,
                    CreatedAt = DateTime.Now
                };
                await _clientRepo.AddAsync(client);
                await _clientRepo.SaveChangesAsync();
            }

            var appointment = new Appointment
            {
                ClientId = client.Id,
                ServiceId = model.ServiceId,
                DateStart = time,
                DateEnd = dateEnd,
                CreatedAt = DateTime.Now
            };

            await _appointmentRepo.AddAsync(appointment);
            await _appointmentRepo.SaveChangesAsync();

            TempData["Message"] = $"✅ Вы успешно записаны! {service.Name} — {time:dd.MM.yyyy HH:mm}";
            TempData["IsSuccess"] = true;

            return RedirectToAction("Index");
        }

        // ===== ВСПОМОГАТЕЛЬНЫЕ МЕТОДЫ =====

        private async Task<List<Slot>> GetSlots(DateTime date, int serviceId, List<Service> services)
        {
            var slots = new List<Slot>();
            var service = services.FirstOrDefault(x => x.Id == serviceId);
            if (service == null) return slots;

            if (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday)
                return slots;

            // Проверяем блокировки
            var blockedSlots = (await _blockedSlotRepo.FindAsync(b => b.Date.Date == date.Date)).ToList();
            if (blockedSlots.Any(b => b.BlockedHour == null))
                return slots; // весь день заблокирован

            var blockedHours = blockedSlots
                .Where(b => b.BlockedHour != null)
                .Select(b => b.BlockedHour!.Value)
                .ToHashSet();

            var workDayEnd = date.Date.AddHours(EndHour);
            var appointments = (await _appointmentRepo.FindAsync(a => a.DateStart.Date == date.Date)).ToList();

            for (int h = StartHour; h < EndHour; h++)
            {
                var slotTime = date.Date.AddHours(h);
                var slotEnd = slotTime.AddMinutes(service.DurationMinutes);

                if (slotEnd > workDayEnd)
                {
                    slots.Add(new Slot { Time = slotTime, IsBusy = true });
                    continue;
                }

                bool conflict = blockedHours.Contains(h) ||
                                appointments.Any(a => slotTime < a.DateEnd && slotEnd > a.DateStart);

                slots.Add(new Slot { Time = slotTime, IsBusy = conflict });
            }

            return slots;
        }

        private async Task<List<DateTime>> GetAvailableDates(int serviceId, List<Service> services)
        {
            var available = new List<DateTime>();
            var service = services.FirstOrDefault(x => x.Id == serviceId);
            if (service == null) return available;

            // Загружаем все блокировки за период сразу
            var allBlocked = (await _blockedSlotRepo.GetAllAsync())
                .Where(b => b.Date >= DateTime.Today && b.Date <= DateTime.Today.AddDays(BookingDays))
                .ToList();

            for (int i = 0; i < BookingDays; i++)
            {
                var date = DateTime.Today.AddDays(i);

                if (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday)
                    continue;

                // Весь день заблокирован
                var dayBlocked = allBlocked.Any(b => b.Date.Date == date && b.BlockedHour == null);
                if (dayBlocked) continue;

                var blockedHours = allBlocked
                    .Where(b => b.Date.Date == date && b.BlockedHour != null)
                    .Select(b => b.BlockedHour!.Value)
                    .ToHashSet();

                var workDayEnd = date.Date.AddHours(EndHour);
                var appointments = (await _appointmentRepo.FindAsync(a => a.DateStart.Date == date.Date)).ToList();

                for (int h = StartHour; h < EndHour; h++)
                {
                    var slotTime = date.Date.AddHours(h);
                    var slotEnd = slotTime.AddMinutes(service.DurationMinutes);

                    if (slotTime < DateTime.Now) continue;
                    if (slotEnd > workDayEnd) continue;
                    if (blockedHours.Contains(h)) continue;

                    bool conflict = appointments.Any(a => slotTime < a.DateEnd && slotEnd > a.DateStart);
                    if (!conflict) { available.Add(date); break; }
                }
            }

            return available;
        }

        public class Slot
        {
            public DateTime Time { get; set; }
            public bool IsBusy { get; set; }
        }
    }
}
