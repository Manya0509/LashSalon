using ClosedXML.Excel;
using LashBooking.Domain.Constants;
using LashBooking.Domain.Entities;
using LashBooking.Domain.Interfaces;
using LashBooking.Reports;
using LashBooking.Reports.Models;
using LashBooking.Web.MVC.Filters;
using Microsoft.AspNetCore.Mvc;

namespace LashBooking.Web.MVC.Controllers
{
    [RequireAdminAuth]
    public class AdminController : BaseController
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
            IConfiguration configuration,
            ILogger logger) : base(logger)
        {
            _appointments = appointments;
            _clients = clients;
            _services = services;
            _reviews = reviews;
            _blockedSlots = blockedSlots;
            _adminPassword = configuration["AdminPassword"]
                ?? throw new InvalidOperationException("Пароль администратора не настроен.");
        }

        // ===== АВТОРИЗАЦИЯ =====

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
            try
            {
                if (adminPassword == _adminPassword)
                {
                    HttpContext.Session.SetString("IsAdmin", "true");
                    return RedirectToAction("Index");
                }
                // Неверный пароль — Warning, не ошибка приложения
                return RedirectToAction("Login", new { error = "Неверный пароль" });
            }
            catch (Exception ex)
            {
                CatchException(ex, "AdminController/Authorize", ErrorLevel.Error);
                return RedirectToAction("Login", new { error = "Ошибка авторизации" });
            }
        }

        [SkipRequireAdminAuth]
        [HttpGet]
        public IActionResult Logout()
        {
            HttpContext.Session.Remove("IsAdmin");
            return RedirectToAction("Login");
        }

        // ===== ГЛАВНАЯ =====

        [HttpGet]
        public async Task<IActionResult> Index(string tab = "appointments")
        {
            try
            {
                InitRequestInfo();

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

                if (TempData["Message"] != null)
                {
                    ViewBag.Message = TempData["Message"];
                    ViewBag.IsSuccess = TempData["IsSuccess"] ?? true;
                }

                return View();
            }
            catch (Exception ex)
            {
                CatchException(ex, "AdminController/Index", ErrorLevel.Error);
                return RedirectToAction("Login");
            }
        }

        // ===== ЗАПИСИ =====

        [HttpGet]
        public async Task<IActionResult> FilterAppointments(string filter = "today", string search = "")
        {
            try
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
            catch (Exception ex)
            {
                CatchException(ex, "AdminController/FilterAppointments", ErrorLevel.Warning);
                return RedirectToAction("Index", new { tab = "appointments" });
            }
        }

        private async Task LoadAppointmentsTab(
            IEnumerable<Appointment> all,
            IEnumerable<Client> clients,
            IEnumerable<Service> services,
            string filter,
            string search)
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

            var list = filtered.OrderBy(a => a.DateStart).ToList();
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
            try
            {
                var appt = await _appointments.GetByIdAsync(appointmentId);
                if (appt == null) return Json(new { success = false, message = "Запись не найдена" });

                appt.Status = (AppointmentStatus)status;
                _appointments.Update(appt);
                await _appointments.SaveChangesAsync();
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                CatchException(ex, "AdminController/UpdateAppointmentStatus", ErrorLevel.Error);
                return Json(new { success = false, message = "Ошибка при обновлении статуса" });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveAppointment(int appointmentId, DateTime dateStart, int status, string? notes)
        {
            try
            {
                var appt = await _appointments.GetByIdAsync(appointmentId);
                if (appt == null)
                {
                    TempData["Message"] = "Запись не найдена.";
                    TempData["IsSuccess"] = false;
                    return RedirectToAction("Index", new { tab = "appointments" });
                }

                appt.DateStart = dateStart;
                appt.DateEnd = dateStart.AddMinutes(60);
                appt.Status = (AppointmentStatus)status;
                appt.Notes = notes;
                _appointments.Update(appt);
                await _appointments.SaveChangesAsync();

                TempData["Message"] = "Запись обновлена.";
                TempData["IsSuccess"] = true;
                return RedirectToAction("Index", new { tab = "appointments" });
            }
            catch (Exception ex)
            {
                CatchException(ex, "AdminController/SaveAppointment", ErrorLevel.Error);
                TempData["Message"] = "Ошибка при сохранении записи.";
                TempData["IsSuccess"] = false;
                return RedirectToAction("Index", new { tab = "appointments" });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteAppointment(int appointmentId)
        {
            try
            {
                var appt = await _appointments.GetByIdAsync(appointmentId);
                if (appt == null)
                {
                    TempData["Message"] = "Запись не найдена.";
                    TempData["IsSuccess"] = false;
                    return RedirectToAction("Index", new { tab = "appointments" });
                }

                _appointments.Delete(appt);
                await _appointments.SaveChangesAsync();
                TempData["Message"] = "Запись удалена.";
                TempData["IsSuccess"] = true;
                return RedirectToAction("Index", new { tab = "appointments" });
            }
            catch (Exception ex)
            {
                CatchException(ex, "AdminController/DeleteAppointment", ErrorLevel.Error);
                TempData["Message"] = "Ошибка при удалении записи.";
                TempData["IsSuccess"] = false;
                return RedirectToAction("Index", new { tab = "appointments" });
            }
        }

        // ===== ОТЗЫВЫ =====

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
            try
            {
                var r = await _reviews.GetByIdAsync(reviewId);
                if (r == null)
                {
                    TempData["Message"] = "Отзыв не найден.";
                    TempData["IsSuccess"] = false;
                    return RedirectToAction("Index", new { tab = "reviews" });
                }

                r.IsApproved = true;
                _reviews.Update(r);
                await _reviews.SaveChangesAsync();
                TempData["Message"] = "Отзыв одобрен.";
                TempData["IsSuccess"] = true;
                return RedirectToAction("Index", new { tab = "reviews" });
            }
            catch (Exception ex)
            {
                CatchException(ex, "AdminController/ApproveReview", ErrorLevel.Error);
                TempData["Message"] = "Ошибка при одобрении отзыва.";
                TempData["IsSuccess"] = false;
                return RedirectToAction("Index", new { tab = "reviews" });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RejectReview(int reviewId)
        {
            try
            {
                var r = await _reviews.GetByIdAsync(reviewId);
                if (r == null)
                {
                    TempData["Message"] = "Отзыв не найден.";
                    TempData["IsSuccess"] = false;
                    return RedirectToAction("Index", new { tab = "reviews" });
                }

                r.IsApproved = false;
                _reviews.Update(r);
                await _reviews.SaveChangesAsync();
                TempData["Message"] = "Отзыв снят с публикации.";
                TempData["IsSuccess"] = true;
                return RedirectToAction("Index", new { tab = "reviews" });
            }
            catch (Exception ex)
            {
                CatchException(ex, "AdminController/RejectReview", ErrorLevel.Error);
                TempData["Message"] = "Ошибка при отклонении отзыва.";
                TempData["IsSuccess"] = false;
                return RedirectToAction("Index", new { tab = "reviews" });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteReview(int reviewId)
        {
            try
            {
                var r = await _reviews.GetByIdAsync(reviewId);
                if (r == null)
                {
                    TempData["Message"] = "Отзыв не найден.";
                    TempData["IsSuccess"] = false;
                    return RedirectToAction("Index", new { tab = "reviews" });
                }

                _reviews.Delete(r);
                await _reviews.SaveChangesAsync();
                TempData["Message"] = "Отзыв удалён.";
                TempData["IsSuccess"] = true;
                return RedirectToAction("Index", new { tab = "reviews" });
            }
            catch (Exception ex)
            {
                CatchException(ex, "AdminController/DeleteReview", ErrorLevel.Error);
                TempData["Message"] = "Ошибка при удалении отзыва.";
                TempData["IsSuccess"] = false;
                return RedirectToAction("Index", new { tab = "reviews" });
            }
        }

        // ===== КЛИЕНТЫ =====

        [HttpGet]
        public async Task<IActionResult> FilterClients(string search = "")
        {
            try
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
            catch (Exception ex)
            {
                CatchException(ex, "AdminController/FilterClients", ErrorLevel.Warning);
                return RedirectToAction("Index", new { tab = "clients" });
            }
        }

        private void LoadClientsTab(
            IEnumerable<Client> clients,
            IEnumerable<Appointment> appointments,
            string search)
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

        // ===== УСЛУГИ =====

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveService(
            int serviceId, string name, string? description, decimal price, int duration)
        {
            try
            {
                if (serviceId == 0)
                {
                    await _services.AddAsync(new Service
                    {
                        Name = name,
                        Description = description,
                        Price = price,
                        DurationMinutes = duration,
                        IsActive = true
                    });
                }
                else
                {
                    var svc = await _services.GetByIdAsync(serviceId);
                    if (svc == null)
                    {
                        TempData["Message"] = "Услуга не найдена.";
                        TempData["IsSuccess"] = false;
                        return RedirectToAction("Index", new { tab = "services" });
                    }
                    svc.Name = name;
                    svc.Description = description;
                    svc.Price = price;
                    svc.DurationMinutes = duration;
                    _services.Update(svc);
                }

                await _services.SaveChangesAsync();
                TempData["Message"] = serviceId == 0 ? "Услуга добавлена." : "Услуга обновлена.";
                TempData["IsSuccess"] = true;
                return RedirectToAction("Index", new { tab = "services" });
            }
            catch (Exception ex)
            {
                CatchException(ex, "AdminController/SaveService", ErrorLevel.Error);
                TempData["Message"] = "Ошибка при сохранении услуги.";
                TempData["IsSuccess"] = false;
                return RedirectToAction("Index", new { tab = "services" });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleService(int serviceId)
        {
            try
            {
                var svc = await _services.GetByIdAsync(serviceId);
                if (svc == null)
                {
                    TempData["Message"] = "Услуга не найдена.";
                    TempData["IsSuccess"] = false;
                    return RedirectToAction("Index", new { tab = "services" });
                }

                svc.IsActive = !svc.IsActive;
                _services.Update(svc);
                await _services.SaveChangesAsync();
                TempData["Message"] = svc.IsActive ? "Услуга активирована." : "Услуга деактивирована.";
                TempData["IsSuccess"] = true;
                return RedirectToAction("Index", new { tab = "services" });
            }
            catch (Exception ex)
            {
                CatchException(ex, "AdminController/ToggleService", ErrorLevel.Error);
                TempData["Message"] = "Ошибка при изменении статуса услуги.";
                TempData["IsSuccess"] = false;
                return RedirectToAction("Index", new { tab = "services" });
            }
        }

        // ===== РАСПИСАНИЕ =====

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
            try
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
            catch (Exception ex)
            {
                CatchException(ex, "AdminController/BlockSlot", ErrorLevel.Error);
                TempData["Message"] = "Ошибка при блокировке слота.";
                TempData["IsSuccess"] = false;
                return RedirectToAction("Index", new { tab = "schedule" });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UnblockSlot(int slotId)
        {
            try
            {
                var slot = await _blockedSlots.GetByIdAsync(slotId);
                if (slot == null)
                {
                    TempData["Message"] = "Слот не найден.";
                    TempData["IsSuccess"] = false;
                    return RedirectToAction("Index", new { tab = "schedule" });
                }

                _blockedSlots.Delete(slot);
                await _blockedSlots.SaveChangesAsync();
                TempData["Message"] = "Блокировка снята.";
                TempData["IsSuccess"] = true;
                return RedirectToAction("Index", new { tab = "schedule" });
            }
            catch (Exception ex)
            {
                CatchException(ex, "AdminController/UnblockSlot", ErrorLevel.Error);
                TempData["Message"] = "Ошибка при снятии блокировки.";
                TempData["IsSuccess"] = false;
                return RedirectToAction("Index", new { tab = "schedule" });
            }
        }

        // ===== ЭКСПОРТ =====

        // ===== ОТЧЁТ PDF (DevExpress) =====

        [HttpGet]
        public async Task<IActionResult> ReportAppointments(string? from, string? to)
        {
            try
            {
                // Определяем период — по умолчанию текущий месяц
                DateTime dateFrom;
                DateTime dateTo;

                if (!string.IsNullOrEmpty(from) && DateTime.TryParse(from, out var parsedFrom))
                    dateFrom = parsedFrom;
                else
                    dateFrom = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);

                if (!string.IsNullOrEmpty(to) && DateTime.TryParse(to, out var parsedTo))
                    dateTo = parsedTo;
                else
                    dateTo = dateFrom.AddMonths(1).AddDays(-1);

                // Загружаем данные
                var appointments = await _appointments.GetAllAsync();
                var clients = (await _clients.GetAllAsync()).ToDictionary(c => c.Id);
                var services = (await _services.GetAllAsync()).ToDictionary(s => s.Id);

                // Фильтруем по периоду
                var filtered = appointments
                    .Where(a => a.DateStart.Date >= dateFrom && a.DateStart.Date <= dateTo)
                    .OrderBy(a => a.DateStart)
                    .ToList();

                // Формируем модель для отчёта
                // Считаем итоговую сумму по завершённым записям
                var totalSum = filtered
                    .Where(a => a.Status == AppointmentStatus.Completed)
                    .Sum(a => services.TryGetValue(a.ServiceId, out var s) ? s.Price : 0);

                var model = new AppointmentReportModel
                {
                    Name = $"Отчёт по записям: {dateFrom:dd.MM.yyyy} — {dateTo:dd.MM.yyyy}",
                    TotalSum = $"{totalSum:N2} ₽",
                    Rows = filtered.Select(a =>
                    {
                        clients.TryGetValue(a.ClientId, out var client);
                        services.TryGetValue(a.ServiceId, out var service);

                        return new AppointmentReportRow
                        {
                            Date = a.DateStart.ToString("dd.MM.yyyy"),
                            Time = a.DateStart.ToString("HH:mm"),
                            ClientName = client?.Name ?? "—",
                            ClientPhone = client?.Phone ?? "—",
                            ServiceName = service?.Name ?? "—",
                            Duration = $"{service?.DurationMinutes ?? 0} мин",
                            Price = $"{service?.Price ?? 0} ₽",
                            Status = a.Status switch
                            {
                                AppointmentStatus.Scheduled => "Запланирована",
                                AppointmentStatus.Confirmed => "Подтверждена",
                                AppointmentStatus.Completed => "Выполнена",
                                AppointmentStatus.Cancelled => "Отменена",
                                AppointmentStatus.NoShow => "Не явился",
                                _ => "—"
                            }
                        };
                    }).ToList()
                };

                // Генерируем PDF
                var engine = new ReportEngine();
                byte[] file = engine.GenerateAppointmentReport(model, "pdf");

                // Отдаём файл браузеру на скачивание
                string fileName = $"Отчёт_{dateFrom:dd-MM-yyyy}_{dateTo:dd-MM-yyyy}.pdf";
                return File(file,
                    "application/pdf",
                    fileName);
            }
            catch (Exception ex)
            {
                CatchException(ex, "AdminController/ReportAppointments", ErrorLevel.Error);
                TempData["Message"] = "Ошибка при создании отчёта.";
                TempData["IsSuccess"] = false;
                return RedirectToAction("Index", new { tab = "appointments" });
            }
        }
    }
}
