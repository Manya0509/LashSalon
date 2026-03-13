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

        private const int StartHour = 9;
        private const int EndHour = 18;
        private const int BookingDays = 30;

        public BookingController(
            IRepository<Service> serviceRepo,
            IRepository<Client> clientRepo,
            IRepository<Appointment> appointmentRepo)
        {
            _serviceRepo = serviceRepo;
            _clientRepo = clientRepo;
            _appointmentRepo = appointmentRepo;
        }

        // GET: /Booking?date=2026-03-15T09:00:00
        public async Task<IActionResult> Index(string? date)
        {
            var services = (await _serviceRepo.GetAllAsync()).ToList();
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

        // GET: /Booking/GetSlotsByDate?date=2026-03-15&serviceId=1
        public async Task<IActionResult> GetSlotsByDate(string date, int serviceId)
        {
            if (!DateTime.TryParse(date, out DateTime selectedDate))
                return Json(new List<object>());

            var services = (await _serviceRepo.GetAllAsync()).ToList();
            var slots = await GetSlots(selectedDate, serviceId, services);

            return Json(slots.Select(s => new
            {
                time = s.Time.ToString("yyyy-MM-ddTHH:mm:ss"),
                timeDisplay = s.Time.ToString("HH:mm"),
                isBusy = s.IsBusy
            }));
        }

        // GET: /Booking/GetAvailableDatesAction?serviceId=1
        public async Task<IActionResult> GetAvailableDatesAction(int serviceId)
        {
            var services = (await _serviceRepo.GetAllAsync()).ToList();
            var dates = await GetAvailableDates(serviceId, services);
            return Json(dates.Select(d => d.ToString("yyyy-MM-dd")));
        }

        // POST: /Booking/Save
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Save(BookingViewModel model)
        {
            // Проверка атрибутов валидации
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .FirstOrDefault();

                TempData["Message"] = errors ?? "Пожалуйста, заполните все поля";
                TempData["IsSuccess"] = false;
                return RedirectToAction("Index");
            }

            // Проверка времени
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

            // Проверка на дубликат
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

            // Поиск или создание клиента
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

            // Создание записи
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

            var appointments = (await _appointmentRepo.FindAsync(a =>
                a.ServiceId == serviceId &&
                a.DateStart.Date == date.Date)).ToList();

            for (int h = StartHour; h < EndHour; h++)
            {
                var slotTime = date.Date.AddHours(h);
                var slotEnd = slotTime.AddMinutes(service.DurationMinutes);
                var conflict = appointments.Any(a =>
                    slotTime < a.DateEnd && slotEnd > a.DateStart);

                slots.Add(new Slot { Time = slotTime, IsBusy = conflict });
            }

            return slots;
        }

        private async Task<List<DateTime>> GetAvailableDates(int serviceId, List<Service> services)
        {
            var available = new List<DateTime>();
            var service = services.FirstOrDefault(x => x.Id == serviceId);
            if (service == null) return available;

            for (int i = 0; i < BookingDays; i++)
            {
                var date = DateTime.Today.AddDays(i);
                var appointments = (await _appointmentRepo.FindAsync(a =>
                    a.ServiceId == serviceId &&
                    a.DateStart.Date == date.Date)).ToList();

                for (int h = StartHour; h < EndHour; h++)
                {
                    var slotTime = date.Date.AddHours(h);
                    var slotEnd = slotTime.AddMinutes(service.DurationMinutes);
                    if (slotTime < DateTime.Now) continue;
                    var conflict = appointments.Any(a =>
                        slotTime < a.DateEnd && slotEnd > a.DateStart);
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
