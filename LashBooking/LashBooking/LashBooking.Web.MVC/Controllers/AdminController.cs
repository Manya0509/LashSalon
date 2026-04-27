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
        private readonly IRepository<GalleryPhoto> _galleryPhotos;
        private readonly IRepository<AboutInfo> _aboutInfos;
        private readonly IWebHostEnvironment _env;

        private readonly string _adminPassword;

        public AdminController(
            IRepository<Appointment> appointments,
            IRepository<Client> clients,
            IRepository<Service> services,
            IRepository<Review> reviews,
            IRepository<BlockedSlot> blockedSlots,
            IRepository<GalleryPhoto> galleryPhotos,
            IRepository<AboutInfo> aboutInfos,
            IWebHostEnvironment env,
            IConfiguration configuration,
            ILogger logger) : base(logger)
        {
            _appointments = appointments;
            _clients = clients;
            _services = services;
            _reviews = reviews;
            _blockedSlots = blockedSlots;
            _galleryPhotos = galleryPhotos;
            _aboutInfos = aboutInfos;
            _env = env;
            _adminPassword = configuration["AdminPassword"]
                ?? throw new InvalidOperationException("Пароль администратора не настроен.");
        }


        // ===== АВТОРИЗАЦИЯ =====

        [SkipRequireAdminAuth]
        [HttpGet]
        public IActionResult Login()
        {
            if (HttpContext.Session.GetString("IsAdmin") == "true")
                return RedirectToAction("Index");
            return Redirect("/Auth/Login");
        }

        [SkipRequireAdminAuth]
        [HttpPost]
        public IActionResult Authorize()
        {
            return Redirect("/Auth/Login");
        }

        [SkipRequireAdminAuth]
        [HttpGet]
        public IActionResult Logout()
        {
            HttpContext.Session.Remove("IsAdmin");
            HttpContext.Session.Remove("ClientId");
            HttpContext.Session.Remove("ClientName");
            return Redirect("/Auth/Login");
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
                ViewBag.TotalClients = allClients.Count(c => !c.IsDeleted);
                ViewBag.ActiveTab = tab;

                if (tab == "appointments") await LoadAppointmentsTab(allAppointments, allClients, allServices, "today", "");
                else if (tab == "reviews") LoadReviewsTab(allReviews, allClients);
                else if (tab == "clients") LoadClientsTab(allClients, allAppointments, "");
                else if (tab == "services") ViewBag.Services = allServices.OrderBy(s => s.Name).ToList();
                else if (tab == "schedule") await LoadScheduleTab();
                else if (tab == "gallery") await LoadGalleryTab();
                else if (tab == "about") await LoadAboutTab();

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
                ViewBag.TotalClients = allClients.Count(c => !c.IsDeleted);
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
                var cIds = clients.Where(c => c.Name.ToLower().Contains(s) || c.Phone.Contains(s))
                   .Select(c => c.Id).ToHashSet();
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveClientNotes(int clientId, string? notes)
        {
            var client = await _clients.GetByIdAsync(clientId);
            if (client == null)
                return RedirectToAction("Index", new { tab = "clients" });

            client.Notes = notes;
            _clients.Update(client);
            await _clients.SaveChangesAsync();

            return RedirectToAction("Index", new { tab = "clients" });
        }

        public async Task<IActionResult> DeleteClient(int clientId)
        {
            var client = await _clients.GetByIdAsync(clientId);
            if (client == null) { /* TempData ошибка, return */ }

            var allAppointments = await _appointments.GetAllAsync();
            int apptCount = allAppointments.Count(a => a.ClientId == clientId);

            client.IsDeleted = true;
            _clients.Update(client);
            await _clients.SaveChangesAsync();

            TempData["Message"] = apptCount > 0
                ? $"Клиент «{client.Name}» удалён. {apptCount} записей сохранены в истории."
                : $"Клиент «{client.Name}» удалён.";
            TempData["IsSuccess"] = true;
            return RedirectToAction("Index", new { tab = "clients" });
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
            ViewBag.PendingReviewsList = list
    .Where(r => !r.IsApproved && !r.IsRejected)
    .OrderByDescending(r => r.CreatedAt).ToList();
            ViewBag.ApprovedReviewsList = list
                .Where(r => r.IsApproved)
                .OrderByDescending(r => r.CreatedAt).ToList();
            ViewBag.RejectedReviewsList = list
                .Where(r => r.IsRejected)
                .OrderByDescending(r => r.CreatedAt).ToList();
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
                r.IsRejected = false;
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
                r.IsRejected = true;
                _reviews.Update(r);
                await _reviews.SaveChangesAsync();
                TempData["Message"] = "Отзыв отклонён.";
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UnpublishReview(int reviewId)
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
                r.IsRejected = false;   // возвращаем в «На модерации»
                _reviews.Update(r);
                await _reviews.SaveChangesAsync();
                TempData["Message"] = "Отзыв возвращён на модерацию.";
                TempData["IsSuccess"] = true;
                return RedirectToAction("Index", new { tab = "reviews" });
            }
            catch (Exception ex)
            {
                CatchException(ex, "AdminController/UnpublishReview", ErrorLevel.Error);
                TempData["Message"] = "Ошибка.";
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
            var list = clients.Where(c => !c.IsDeleted);
            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.ToLower();
                list = list.Where(c => c.Name.ToLower().Contains(s) || c.Phone.Contains(s));
            }
            var counts = appointments.GroupBy(a => a.ClientId).ToDictionary(g => g.Key, g => g.Count());
            ViewBag.Clients = list.OrderBy(c => c.Name).ToList();
            ViewBag.AppointmentCounts = counts;
            ViewBag.TotalClients = clients.Count(c => !c.IsDeleted);
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
                    CreatedAt = DateTime.UtcNow
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

        // ===== О СЕБЕ =====
        private async Task LoadAboutTab()
        {
            var all = await _aboutInfos.GetAllAsync();
            var info = all.FirstOrDefault();

            // Если записи ещё нет — подставляем данные, которые сейчас захардкожены на сайте
            if (info == null)
            {
                info = new AboutInfo
                {
                    MasterName = "Дарья Лукина",
                    Role = "Мастер Lash-стилист",
                    Experience = "Опыт работы 5+ лет",
                    Quote = "Ресницы — это не просто красота, это искусство подчеркнуть вашу индивидуальность",
                    AboutText = "Привет! Меня зовут Дарья. Я сертифицированный Lash-стилист с опытом работы более 5 лет. Моя специализация — создание натуральных и выразительных образов с помощью наращивания ресниц.\nЯ постоянно обучаюсь новым техникам, слежу за трендами и использую только премиальные материалы, чтобы каждая клиентка чувствовала себя уверенно и красиво.",
                    EducationText = "Школа LashStudio (2019)\nШкола LashTime (2020)\nСертифицированный мастер бровист (2021)\nСертифицированный мастер по ламинированию бровей (2021)",
                    Address = "г. Чита, ул. Журавлева, д. 72",
                    WorkingHours = "Ежедневно с 9:00 до 18:00",
                    Phone = "+7 (999) 410-38-01",
                    WhatsAppLink = "https://wa.me/79994103801",
                    TelegramLink = "https://t.me/79994103801",
                    StudioName = "Студия LashLukina"
                };
            }

            ViewBag.AboutInfo = info;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveAboutInfo(
    string masterName, string role, string experience,
    string quote, string aboutText, string educationText,
    string address, string workingHours, string phone,
    string? whatsAppLink, string? telegramLink,
    string? studioName,
    IFormFile? photo, IFormFile? heroPhoto)
        {
            try
            {
                // Получаем все записи из таблицы (там 0 или 1 запись)
                var all = await _aboutInfos.GetAllAsync();
                var info = all.FirstOrDefault();

                if (info == null)
                {
                    // Записи ещё нет — создаём новую
                    info = new AboutInfo();
                    // Заполняем все поля из формы
                    info.MasterName = masterName;
                    info.Role = role;
                    info.Experience = experience;
                    info.Quote = quote;
                    info.AboutText = aboutText;
                    info.EducationText = educationText;
                    info.Address = address;
                    info.WorkingHours = workingHours;
                    info.Phone = phone;
                    info.WhatsAppLink = whatsAppLink;
                    info.TelegramLink = telegramLink;
                    info.StudioName = studioName;

                    // Если загружено фото — сохраняем файл
                    if (photo != null && photo.Length > 0)
                    {
                        var ext = Path.GetExtension(photo.FileName).ToLower();
                        var fileName = $"{Guid.NewGuid()}{ext}";
                        var filePath = Path.Combine(_env.WebRootPath, "images", fileName);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await photo.CopyToAsync(stream);
                        }

                        info.PhotoFileName = fileName;
                    }
                    // Если загружено фото фона главной
                    if (heroPhoto != null && heroPhoto.Length > 0)
                    {
                        var heroExt = Path.GetExtension(heroPhoto.FileName).ToLower();
                        var heroFileName = $"{Guid.NewGuid()}{heroExt}";
                        var heroFilePath = Path.Combine(_env.WebRootPath, "images", heroFileName);

                        using (var stream = new FileStream(heroFilePath, FileMode.Create))
                        {
                            await heroPhoto.CopyToAsync(stream);
                        }

                        info.HeroPhotoFileName = heroFileName;
                    }

                    await _aboutInfos.AddAsync(info);
                }
                else
                {
                    // Запись уже существует — обновляем поля
                    info.MasterName = masterName;
                    info.Role = role;
                    info.Experience = experience;
                    info.Quote = quote;
                    info.AboutText = aboutText;
                    info.EducationText = educationText;
                    info.Address = address;
                    info.WorkingHours = workingHours;
                    info.Phone = phone;
                    info.WhatsAppLink = whatsAppLink;
                    info.TelegramLink = telegramLink;
                    info.StudioName = studioName;

                    // Если загружено новое фото — заменяем
                    if (photo != null && photo.Length > 0)
                    {
                        // Удаляем старое фото, если оно есть
                        if (!string.IsNullOrEmpty(info.PhotoFileName))
                        {
                            var oldPath = Path.Combine(_env.WebRootPath, "images", info.PhotoFileName);
                            if (System.IO.File.Exists(oldPath))
                            {
                                System.IO.File.Delete(oldPath);
                            }
                        }

                        var ext = Path.GetExtension(photo.FileName).ToLower();
                        var fileName = $"{Guid.NewGuid()}{ext}";
                        var filePath = Path.Combine(_env.WebRootPath, "images", fileName);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await photo.CopyToAsync(stream);
                        }

                        info.PhotoFileName = fileName;
                    }
                    // Если загружено новое фото фона главной
                    if (heroPhoto != null && heroPhoto.Length > 0)
                    {
                        if (!string.IsNullOrEmpty(info.HeroPhotoFileName))
                        {
                            var oldHeroPath = Path.Combine(_env.WebRootPath, "images", info.HeroPhotoFileName);
                            if (System.IO.File.Exists(oldHeroPath))
                            {
                                System.IO.File.Delete(oldHeroPath);
                            }
                        }

                        var heroExt = Path.GetExtension(heroPhoto.FileName).ToLower();
                        var heroFileName = $"{Guid.NewGuid()}{heroExt}";
                        var heroFilePath = Path.Combine(_env.WebRootPath, "images", heroFileName);

                        using (var stream = new FileStream(heroFilePath, FileMode.Create))
                        {
                            await heroPhoto.CopyToAsync(stream);
                        }

                        info.HeroPhotoFileName = heroFileName;
                    }

                    _aboutInfos.Update(info);
                }

                await _aboutInfos.SaveChangesAsync();

                TempData["Message"] = "Информация сохранена.";
                TempData["IsSuccess"] = true;
                return RedirectToAction("Index", new { tab = "about" });
            }
            catch (Exception ex)
            {
                CatchException(ex, "AdminController/SaveAboutInfo", ErrorLevel.Error);
                TempData["Message"] = "Ошибка при сохранении.";
                TempData["IsSuccess"] = false;
                return RedirectToAction("Index", new { tab = "about" });
            }
        }

        // ===== ГАЛЕРЕЯ =====
        private async Task LoadGalleryTab()
        {
            var photos = await _galleryPhotos.GetAllAsync();
            ViewBag.GalleryPhotos = photos.OrderBy(p => p.SortOrder).ThenByDescending(p => p.UploadedAt).ToList();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadGalleryPhoto(IFormFile photo, string? description)
        {
            try
            {
                if (photo == null || photo.Length == 0)
                {
                    TempData["Message"] = "Файл не выбран.";
                    TempData["IsSuccess"] = false;
                    return RedirectToAction("Index", new { tab = "gallery" });
                }

                var ext = Path.GetExtension(photo.FileName).ToLower();
                var fileName = $"{Guid.NewGuid()}{ext}";

                var galleryPath = Path.Combine(_env.WebRootPath, "images", "gallery");
                Directory.CreateDirectory(galleryPath);
                var filePath = Path.Combine(galleryPath, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await photo.CopyToAsync(stream);
                }

                var allPhotos = await _galleryPhotos.GetAllAsync();
                var maxOrder = allPhotos.Any() ? allPhotos.Max(p => p.SortOrder) : 0;

                var galleryPhoto = new GalleryPhoto
                {
                    FileName = fileName,
                    Description = description,
                    SortOrder = maxOrder + 1,
                    UploadedAt = DateTime.UtcNow
                };
                await _galleryPhotos.AddAsync(galleryPhoto);
                await _galleryPhotos.SaveChangesAsync();

                TempData["Message"] = "Фото загружено.";
                TempData["IsSuccess"] = true;
                return RedirectToAction("Index", new { tab = "gallery" });
            }
            catch (Exception ex)
            {
                CatchException(ex, "AdminController/UploadGalleryPhoto", ErrorLevel.Error);
                TempData["Message"] = "Ошибка при загрузке фото.";
                TempData["IsSuccess"] = false;
                return RedirectToAction("Index", new { tab = "gallery" });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteGalleryPhoto(int photoId)
        {
            try
            {
                var photo = await _galleryPhotos.GetByIdAsync(photoId);
                if (photo == null)
                {
                    TempData["Message"] = "Фото не найдено.";
                    TempData["IsSuccess"] = false;
                    return RedirectToAction("Index", new { tab = "gallery" });
                }

                var filePath = Path.Combine(_env.WebRootPath, "images", "gallery", photo.FileName);
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }

                _galleryPhotos.Delete(photo);
                await _galleryPhotos.SaveChangesAsync();

                TempData["Message"] = "Фото удалено.";
                TempData["IsSuccess"] = true;
                return RedirectToAction("Index", new { tab = "gallery" });
            }
            catch (Exception ex)
            {
                CatchException(ex, "AdminController/DeleteGalleryPhoto", ErrorLevel.Error);
                TempData["Message"] = "Ошибка при удалении фото.";
                TempData["IsSuccess"] = false;
                return RedirectToAction("Index", new { tab = "gallery" });
            }
        }

        // ===== ЭКСПОРТ =====
        [HttpGet]
        public async Task<IActionResult> ExportAppointments()
        {
            try
            {
                var appointments = await _appointments.GetAllAsync();
                var clients = (await _clients.GetAllAsync()).ToDictionary(c => c.Id);
                var services = (await _services.GetAllAsync()).ToDictionary(s => s.Id);

                using var workbook = new XLWorkbook();
                var worksheet = workbook.Worksheets.Add("Записи");

                worksheet.Cell(1, 1).Value = "Дата";
                worksheet.Cell(1, 2).Value = "Время";
                worksheet.Cell(1, 3).Value = "Клиент";
                worksheet.Cell(1, 4).Value = "Телефон";
                worksheet.Cell(1, 5).Value = "Услуга";
                worksheet.Cell(1, 6).Value = "Длительность (мин)";
                worksheet.Cell(1, 7).Value = "Цена";
                worksheet.Cell(1, 8).Value = "Статус";
                worksheet.Cell(1, 9).Value = "Заметки";

                var headerRow = worksheet.Row(1);
                headerRow.Style.Font.Bold = true;

                int row = 2;
                foreach (var a in appointments.OrderBy(a => a.DateStart))
                {
                    clients.TryGetValue(a.ClientId, out var client);
                    services.TryGetValue(a.ServiceId, out var service);

                    worksheet.Cell(row, 1).Value = a.DateStart.ToString("dd.MM.yyyy");
                    worksheet.Cell(row, 2).Value = a.DateStart.ToString("HH:mm");
                    worksheet.Cell(row, 3).Value = client?.Name ?? "—";
                    worksheet.Cell(row, 4).Value = client?.Phone ?? "—";
                    worksheet.Cell(row, 5).Value = service?.Name ?? "—";
                    worksheet.Cell(row, 6).Value = service?.DurationMinutes ?? 0;
                    worksheet.Cell(row, 7).Value = (double)(service?.Price ?? 0);
                    worksheet.Cell(row, 8).Value = a.Status switch
                    {
                        AppointmentStatus.Scheduled => "Запланирована",
                        AppointmentStatus.Confirmed => "Подтверждена",
                        AppointmentStatus.Completed => "Выполнена",
                        AppointmentStatus.Cancelled => "Отменена",
                        AppointmentStatus.NoShow => "Не явился",
                        _ => "—"
                    };
                    worksheet.Cell(row, 9).Value = a.Notes ?? "";
                    row++;
                }

                worksheet.Columns().AdjustToContents();

                using var stream = new MemoryStream();
                workbook.SaveAs(stream);
                stream.Position = 0;

                return File(stream.ToArray(),
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    $"Записи_{DateTime.Today:dd-MM-yyyy}.xlsx");
            }
            catch (Exception ex)
            {
                CatchException(ex, "AdminController/ExportAppointments", ErrorLevel.Error);
                TempData["Message"] = "Ошибка при экспорте.";
                TempData["IsSuccess"] = false;
                return RedirectToAction("Index", new { tab = "appointments" });
            }
        }


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
                    Name = "Студия LashLukina",
                    Period = $"Отчёт по записям за период: {dateFrom:dd.MM.yyyy} — {dateTo:dd.MM.yyyy}",
                    TotalSum = $"{totalSum:N2} ₽",
                    TotalCount = $"Всего записей: {filtered.Count}",
                    GeneratedDate = $"Сформировано: {DateTime.Now:dd.MM.yyyy HH:mm}",
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

        [HttpGet]
        public async Task<IActionResult> ReportRevenue(int? year)
        {
            try
            {
                int reportYear = year ?? DateTime.Today.Year;

                // Загружаем данные
                var appointments = await _appointments.GetAllAsync();
                var services = (await _services.GetAllAsync()).ToDictionary(s => s.Id);

                // Фильтруем по году
                var yearAppointments = appointments
                    .Where(a => a.DateStart.Year == reportYear)
                    .ToList();

                // Группируем по месяцам
                var monthNames = new[] {
            "", "Январь", "Февраль", "Март", "Апрель", "Май", "Июнь",
            "Июль", "Август", "Сентябрь", "Октябрь", "Ноябрь", "Декабрь"
        };

                var rows = new List<RevenueReportRow>();
                decimal totalRevenue = 0;
                int totalCompleted = 0;

                for (int month = 1; month <= 12; month++)
                {
                    var monthApps = yearAppointments
                        .Where(a => a.DateStart.Month == month)
                        .ToList();

                    if (monthApps.Count == 0) continue;

                    var completed = monthApps
                        .Where(a => a.Status == AppointmentStatus.Completed)
                        .ToList();

                    decimal revenue = completed
                        .Sum(a => services.TryGetValue(a.ServiceId, out var s) ? s.Price : 0);

                    decimal avgCheck = completed.Count > 0 ? revenue / completed.Count : 0;

                    totalRevenue += revenue;
                    totalCompleted += completed.Count;

                    rows.Add(new RevenueReportRow
                    {
                        Month = $"{monthNames[month]} {reportYear}",
                        TotalCount = monthApps.Count.ToString(),
                        CompletedCount = completed.Count.ToString(),
                        Revenue = $"{revenue:N2} ₽",
                        AvgCheck = completed.Count > 0 ? $"{avgCheck:N2} ₽" : "—"
                    });
                }

                decimal totalAvg = totalCompleted > 0 ? totalRevenue / totalCompleted : 0;

                var model = new RevenueReportModel
                {
                    Name = "Студия LashLukina",
                    Period = $"Выручка по месяцам за {reportYear} год",
                    TotalRevenue = $"{totalRevenue:N2} ₽",
                    TotalAvgCheck = $"{totalAvg:N2} ₽",
                    GeneratedDate = $"Сформировано: {DateTime.Now:dd.MM.yyyy HH:mm}",
                    Rows = rows
                };

                var engine = new ReportEngine();
                byte[] file = engine.GenerateRevenueReport(model, "pdf");

                string fileName = $"Выручка_{reportYear}.pdf";
                return File(file, "application/pdf", fileName);
            }
            catch (Exception ex)
            {
                CatchException(ex, "AdminController/ReportRevenue", ErrorLevel.Error);
                TempData["Message"] = "Ошибка при создании отчёта.";
                TempData["IsSuccess"] = false;
                return RedirectToAction("Index", new { tab = "appointments" });
            }
        }
    }
}
