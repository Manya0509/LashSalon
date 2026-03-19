using Microsoft.AspNetCore.Mvc;
using LashBooking.Domain.Entities;
using LashBooking.Domain.Interfaces;
using LashBooking.Web.MVC.Filters;
using ClosedXML.Excel;

namespace LashBooking.Web.MVC.Controllers
{
    [RequireAdminAuth] // ← защищает все методы сразу
    public class AdminController : Controller
    {
        private readonly IRepository<Appointment> _appointments;
        private readonly IRepository<Client> _clients;
        private readonly IRepository<Service> _services;
        private readonly IRepository<Review> _reviews;
        private readonly IRepository<BlockedSlot> _blockedSlots;
        private readonly string _adminPassword;

        public AdminController(
            IRepository<Appointment> appointments,
            IRepository<Client> clients,
            IRepository<Service> services,
            IRepository<Review> reviews,
            IRepository<BlockedSlot> blockedSlots,
            IConfiguration configuration)
        {
            _appointments = appointments;
            _clients = clients;
            _services = services;
            _reviews = reviews;
            _blockedSlots = blockedSlots;
            _adminPassword = configuration["AdminPassword"]
                ?? throw new InvalidOperationException("Пароль администратора не настроен в конфигурации.");
        }

        //  АВТОРИЗАЦИЯ  

        [SkipRequireAdminAuth]
        [HttpGet]
        public IActionResult Login(string? error)
        {
            if (HttpContext.Session.GetString("IsAdmin") == "true")
                return RedirectToAction("Index");
            if (error != null) ViewBag.Error = error;
            return View();
        }

        [SkipRequireAdminAuth]
        [HttpPost]
        public IActionResult Authorize(string adminPassword)
        {
            if (adminPassword == _adminPassword)
            {
                HttpContext.Session.SetString("IsAdmin", "true");
                return RedirectToAction("Index");
            }
            return RedirectToAction("Login", new { error = "Неверный пароль" });
        }

        [SkipRequireAdminAuth]
        [HttpGet]
        public IActionResult Logout()
        {
            HttpContext.Session.Remove("IsAdmin");
            return RedirectToAction("Login");
        }

        // ГЛАВНАЯ 

        [HttpGet]
        public async Task<IActionResult> Index(string tab = "appointments")
        {
            var allAppointments = await _appointments.GetAllAsync();
            var allClients = await _clients.GetAllAsync();
            var allServices = await _services.GetAllAsync();
            var allReviews = await _reviews.GetAllAsync();
            var today = DateTime.Today;

            ViewBag.TodayAppointments = allAppointments.Count(a => a.DateStart.Date == today && a.Status != AppointmentStatus.Cancelled);
            ViewBag.UpcomingAppointments = allAppointments.Count(a => a.DateStart.Date > today && a.Status != AppointmentStatus.Cancelled);
            ViewBag.PendingReviews = allReviews.Count(r => !r.IsApproved);
            ViewBag.TotalClients = allClients.Count();
            ViewBag.ActiveTab = tab;

            if (tab == "appointments") await LoadAppointmentsTab(allAppointments, allClients, allServices, "today", "");
            else if (tab == "reviews") LoadReviewsTab(allReviews, allClients);
            else if (tab == "clients") LoadClientsTab(allClients, allAppointments, "");
            else if (tab == "services") ViewBag.Services = allServices.OrderBy(s => s.Name).ToList();
            else if (tab == "schedule") await LoadScheduleTab();

            if (TempData["Message"] != null) { ViewBag.Message = TempData["Message"]; ViewBag.IsSuccess = TempData["IsSuccess"] ?? true; }

            return View();
        }

        //  ЗАПИСИ 

        [HttpGet]
        public async Task<IActionResult> FilterAppointments(string filter = "today", string search = "")
        {
            var allAppointments = await _appointments.GetAllAsync();
            var allClients = await _clients.GetAllAsync();
            var allServices = await _services.GetAllAsync();
            var allReviews = await _reviews.GetAllAsync();
            var today = DateTime.Today;

            ViewBag.TodayAppointments = allAppointments.Count(a => a.DateStart.Date == today && a.Status != AppointmentStatus.Cancelled);
            ViewBag.UpcomingAppointments = allAppointments.Count(a => a.DateStart.Date > today && a.Status != AppointmentStatus.Cancelled);
            ViewBag.PendingReviews = allReviews.Count(r => !r.IsApproved);
            ViewBag.TotalClients = allClients.Count();
            ViewBag.ActiveTab = "appointments";

            await LoadAppointmentsTab(allAppointments, allClients, allServices, filter, search);
            return View("Index");
        }

        private async Task LoadAppointmentsTab(IEnumerable<Appointment> all, IEnumerable<Client> clients, IEnumerable<Service> services, string filter, string search)
        {
            var today = DateTime.Today;
            var filtered = filter switch
            {
                "today" => all.Where(a => a.DateStart.Date == today),
                "tomorrow" => all.Where(a => a.DateStart.Date == today.AddDays(1)),
                "week" => all.Where(a => a.DateStart.Date >= today && a.DateStart.Date <= today.AddDays(7)),
                _ => all.AsEnumerable()
            };

            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.ToLower();
                var cIds = clients.Where(c => c.Name.ToLower().Contains(s) || c.Phone.Contains(s)).Select(c => c.Id).ToHashSet();
                var svIds = services.Where(sv => sv.Name.ToLower().Contains(s)).Select(sv => sv.Id).ToHashSet();
                filtered = filtered.Where(a => cIds.Contains(a.ClientId) || svIds.Contains(a.ServiceId));
            }

            var list = filtered.OrderByDescending(a => a.DateStart).ToList();
            ViewBag.Appointments = list;
            ViewBag.AppointmentsClientsDict = clients.ToDictionary(c => c.Id);
            ViewBag.AppointmentsServicesDict = services.ToDictionary(s => s.Id);
            ViewBag.AppointmentFilter = filter;
            ViewBag.AppointmentSearch = search;
            ViewBag.AppointmentsTotalCount = list.Count;
            ViewBag.AppointmentsTodayCount = list.Count(a => a.DateStart.Date == today);
            ViewBag.AppointmentsScheduledCount = list.Count(a => a.Status == AppointmentStatus.Scheduled);
            ViewBag.AppointmentsConfirmedCount = list.Count(a => a.Status == AppointmentStatus.Confirmed);
            ViewBag.AppointmentsCompletedCount = list.Count(a => a.Status == AppointmentStatus.Completed);
            ViewBag.AppointmentsCancelledCount = list.Count(a => a.Status == AppointmentStatus.Cancelled);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateAppointmentStatus(int appointmentId, int status)
        {
            var appt = await _appointments.GetByIdAsync(appointmentId);
            if (appt == null) return Json(new { success = false });
            appt.Status = (AppointmentStatus)status;
            _appointments.Update(appt);
            await _appointments.SaveChangesAsync();
            return Json(new { success = true });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveAppointment(int appointmentId, DateTime dateStart, int status, string? notes)
        {
            var appt = await _appointments.GetByIdAsync(appointmentId);
            if (appt != null)
            {
                appt.DateStart = dateStart;
                appt.DateEnd = dateStart.AddMinutes(60);
                appt.Status = (AppointmentStatus)status;
                appt.Notes = notes;
                _appointments.Update(appt);
                await _appointments.SaveChangesAsync();
                TempData["Message"] = "Запись обновлена.";
                TempData["IsSuccess"] = true;
            }
            return RedirectToAction("Index", new { tab = "appointments" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteAppointment(int appointmentId)
        {
            var appt = await _appointments.GetByIdAsync(appointmentId);
            if (appt != null)
            {
                _appointments.Delete(appt);
                await _appointments.SaveChangesAsync();
                TempData["Message"] = "Запись удалена.";
                TempData["IsSuccess"] = true;
            }
            return RedirectToAction("Index", new { tab = "appointments" });
        }

        //  ОТЗЫВЫ 

        private void LoadReviewsTab(IEnumerable<Review> reviews, IEnumerable<Client> clients)
        {
            var dict = clients.ToDictionary(c => c.Id);
            var list = reviews.ToList();
            foreach (var r in list)
                if (r.Client == null && dict.TryGetValue(r.ClientId, out var c))
                    r.Client = c;

            double avg = list.Any(r => r.IsApproved)
                ? list.Where(r => r.IsApproved).Average(r => r.Rating) : 0;

            ViewBag.AllReviews = list.OrderByDescending(r => r.CreatedAt).ToList();
            ViewBag.PendingReviewsList = list.Where(r => !r.IsApproved).OrderByDescending(r => r.CreatedAt).ToList();
            ViewBag.ApprovedReviewsList = list.Where(r => r.IsApproved).OrderByDescending(r => r.CreatedAt).ToList();
            ViewBag.AverageRating = avg;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveReview(int reviewId)
        {
            var r = await _reviews.GetByIdAsync(reviewId);
            if (r != null) { r.IsApproved = true; _reviews.Update(r); await _reviews.SaveChangesAsync(); TempData["Message"] = "Отзыв одобрен."; TempData["IsSuccess"] = true; }
            return RedirectToAction("Index", new { tab = "reviews" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RejectReview(int reviewId)
        {
            var r = await _reviews.GetByIdAsync(reviewId);
            if (r != null) { r.IsApproved = false; _reviews.Update(r); await _reviews.SaveChangesAsync(); TempData["Message"] = "Отзыв снят с публикации."; TempData["IsSuccess"] = true; }
            return RedirectToAction("Index", new { tab = "reviews" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteReview(int reviewId)
        {
            var r = await _reviews.GetByIdAsync(reviewId);
            if (r != null) { _reviews.Delete(r); await _reviews.SaveChangesAsync(); TempData["Message"] = "Отзыв удалён."; TempData["IsSuccess"] = true; }
            return RedirectToAction("Index", new { tab = "reviews" });
        }

        //  КЛИЕНТЫ 

        [HttpGet]
        public async Task<IActionResult> FilterClients(string search = "")
        {
            var allAppointments = await _appointments.GetAllAsync();
            var allClients = await _clients.GetAllAsync();
            var allReviews = await _reviews.GetAllAsync();
            var today = DateTime.Today;

            ViewBag.TodayAppointments = allAppointments.Count(a => a.DateStart.Date == today && a.Status != AppointmentStatus.Cancelled);
            ViewBag.UpcomingAppointments = allAppointments.Count(a => a.DateStart.Date > today && a.Status != AppointmentStatus.Cancelled);
            ViewBag.PendingReviews = allReviews.Count(r => !r.IsApproved);
            ViewBag.TotalClients = allClients.Count();
            ViewBag.ActiveTab = "clients";

            LoadClientsTab(allClients, allAppointments, search);
            return View("Index");
        }

        private void LoadClientsTab(IEnumerable<Client> clients, IEnumerable<Appointment> appointments, string search)
        {
            var list = clients.AsEnumerable();
            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.ToLower();
                list = list.Where(c => c.Name.ToLower().Contains(s) || c.Phone.Contains(s));
            }
            var counts = appointments.GroupBy(a => a.ClientId).ToDictionary(g => g.Key, g => g.Count());
            ViewBag.Clients = list.OrderBy(c => c.Name).ToList();
            ViewBag.AppointmentCounts = counts;
            ViewBag.TotalClientsCount = clients.Count();
            ViewBag.ClientSearch = search;
        }

        //  УСЛУГИ 

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveService(int serviceId, string name, string? description, decimal price, int duration)
        {
            if (serviceId == 0)
                await _services.AddAsync(new Service { Name = name, Description = description, Price = price, DurationMinutes = duration, IsActive = true });
            else
            {
                var svc = await _services.GetByIdAsync(serviceId);
                if (svc != null) { svc.Name = name; svc.Description = description; svc.Price = price; svc.DurationMinutes = duration; _services.Update(svc); }
            }
            await _services.SaveChangesAsync();
            TempData["Message"] = serviceId == 0 ? "Услуга добавлена." : "Услуга обновлена.";
            TempData["IsSuccess"] = true;
            return RedirectToAction("Index", new { tab = "services" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleService(int serviceId)
        {
            var svc = await _services.GetByIdAsync(serviceId);
            if (svc != null)
            {
                svc.IsActive = !svc.IsActive;
                _services.Update(svc);
                await _services.SaveChangesAsync();
                TempData["Message"] = svc.IsActive ? "Услуга активирована." : "Услуга деактивирована.";
                TempData["IsSuccess"] = true;
            }
            return RedirectToAction("Index", new { tab = "services" });
        }

        // РАСПИСАНИЕ

        private async Task LoadScheduleTab()
        {
            var today = DateTime.Today;
            var blocked = (await _blockedSlots.GetAllAsync())
                .Where(b => b.Date >= today)
                .OrderBy(b => b.Date)
                .ThenBy(b => b.BlockedHour)
                .ToList();
            ViewBag.BlockedSlots = blocked;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BlockSlot(DateTime date, int? hour, string? reason)
        {
            var blocked = new BlockedSlot
            {
                Date = date.Date,
                BlockedHour = hour == -1 ? null : hour,
                Reason = reason,
                CreatedAt = DateTime.Now
            };
            await _blockedSlots.AddAsync(blocked);
            await _blockedSlots.SaveChangesAsync();
            TempData["Message"] = hour == -1
                ? $"День {date:dd.MM.yyyy} заблокирован."
                : $"Час {hour}:00 {date:dd.MM.yyyy} заблокирован.";
            TempData["IsSuccess"] = true;
            return RedirectToAction("Index", new { tab = "schedule" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UnblockSlot(int slotId)
        {
            var slot = await _blockedSlots.GetByIdAsync(slotId);
            if (slot != null)
            {
                _blockedSlots.Delete(slot);
                await _blockedSlots.SaveChangesAsync();
                TempData["Message"] = "Блокировка снята.";
                TempData["IsSuccess"] = true;
            }
            return RedirectToAction("Index", new { tab = "schedule" });
        }

        //  ЭКСПОРТ 

        [HttpGet]
        public async Task<IActionResult> ExportAppointments()
        {
            var appointments = await _appointments.GetAllAsync();
            var clients = (await _clients.GetAllAsync()).ToDictionary(c => c.Id);
            var services = (await _services.GetAllAsync()).ToDictionary(s => s.Id);

            using var workbook = new ClosedXML.Excel.XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Записи");

            // Заголовки
            worksheet.Cell(1, 1).Value = "Дата";
            worksheet.Cell(1, 2).Value = "Время";
            worksheet.Cell(1, 3).Value = "Клиент";
            worksheet.Cell(1, 4).Value = "Телефон";
            worksheet.Cell(1, 5).Value = "Услуга";
            worksheet.Cell(1, 6).Value = "Длительность (мин)";
            worksheet.Cell(1, 7).Value = "Цена";
            worksheet.Cell(1, 8).Value = "Статус";
            worksheet.Cell(1, 9).Value = "Заметки";

            // Стиль заголовков
            var headerRow = worksheet.Row(1);
            headerRow.Style.Font.Bold = true;
            headerRow.Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.FromHtml("#f0f0f0");

            // Данные
            int row = 2;
            foreach (var appt in appointments.OrderByDescending(a => a.DateStart))
            {
                clients.TryGetValue(appt.ClientId, out var client);
                services.TryGetValue(appt.ServiceId, out var service);

                string status = appt.Status switch
                {
                    AppointmentStatus.Scheduled => "Запланирована",
                    AppointmentStatus.Confirmed => "Подтверждена",
                    AppointmentStatus.Completed => "Выполнена",
                    AppointmentStatus.Cancelled => "Отменена",
                    AppointmentStatus.NoShow => "Не явился",
                    _ => "—"
                };

                worksheet.Cell(row, 1).Value = appt.DateStart.ToString("dd.MM.yyyy");
                worksheet.Cell(row, 2).Value = appt.DateStart.ToString("HH:mm");
                worksheet.Cell(row, 3).Value = client?.Name ?? "—";
                worksheet.Cell(row, 4).Value = client?.Phone ?? "—";
                worksheet.Cell(row, 5).Value = service?.Name ?? "—";
                worksheet.Cell(row, 6).Value = service?.DurationMinutes ?? 0;
                worksheet.Cell(row, 7).Value = service?.Price ?? 0;
                worksheet.Cell(row, 8).Value = status;
                worksheet.Cell(row, 9).Value = appt.Notes ?? "—";

                row++;
            }

            // Автоширина колонок
            worksheet.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            stream.Position = 0;

            string fileName = $"Записи_{DateTime.Today:dd-MM-yyyy}.xlsx";
            return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }
    }
}
