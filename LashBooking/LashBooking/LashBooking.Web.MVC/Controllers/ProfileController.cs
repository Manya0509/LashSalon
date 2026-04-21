using Microsoft.AspNetCore.Mvc;
using LashBooking.Domain.Entities;
using LashBooking.Domain.Interfaces;
using LashBooking.Domain.Constants;
using LashBooking.Web.MVC.Filters;
using LashBooking.Web.MVC.Services;

namespace LashBooking.Web.MVC.Controllers
{
    [RequireClientAuth]
    public class ProfileController : BaseController
    {
        private readonly IRepository<Appointment> _appointmentRepo;
        private readonly IRepository<Service> _serviceRepo;
        private readonly IRepository<Client> _clientRepo;

        public ProfileController(
            IRepository<Appointment> appointmentRepo,
            IRepository<Service> serviceRepo,
            IRepository<Client> clientRepo,
            ILogger logger) : base(logger)
        {
            _appointmentRepo = appointmentRepo;
            _serviceRepo = serviceRepo;
            _clientRepo = clientRepo;
        }


        public async Task<IActionResult> Index(string filter = "upcoming")
        {
            try
            {
                InitRequestInfo();

                var clientId = HttpContext.Session.GetInt32("ClientId")!.Value;
                var clientName = HttpContext.Session.GetString("ClientName") ?? "Гость";

                var allServices = await _serviceRepo.GetAllAsync();
                var servicesDictionary = allServices
                    .Where(s => s.IsActive)
                    .ToDictionary(s => s.Id, s => s);

                var appointments = await _appointmentRepo.FindAsync(a => a.ClientId == clientId);
                var now = DateTime.Now;

                var filtered = filter switch
                {
                    "upcoming" => appointments.Where(a =>
                        a.DateStart >= now &&
                        (a.Status == AppointmentStatus.Scheduled || a.Status == AppointmentStatus.Confirmed)),
                    "past" => appointments.Where(a =>
                        a.DateStart < now ||
                        a.Status == AppointmentStatus.Completed ||
                        a.Status == AppointmentStatus.Cancelled ||
                        a.Status == AppointmentStatus.NoShow),
                    _ => appointments
                };

                var appointmentList = filtered
                    .OrderByDescending(a => a.DateStart)
                    .ToList();

                ViewBag.ClientName = clientName;
                ViewBag.Appointments = appointmentList;
                ViewBag.ServicesDictionary = servicesDictionary;
                ViewBag.CancelSuccess = TempData["CancelSuccess"];
                ViewBag.CancelError = TempData["CancelError"];
                ViewBag.CurrentFilter = filter;

                return View();
            }
            catch (Exception ex)
            {
                // Ошибка загрузки профиля — Error
                CatchException(ex, "ProfileController/Index", ErrorLevel.Error);
                return RedirectToAction("Index", "Home");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Cancel(int appointmentId)
        {
            try
            {
                InitRequestInfo();

                var clientId = HttpContext.Session.GetInt32("ClientId")!.Value;

                var appointments = await _appointmentRepo.FindAsync(a =>
                    a.Id == appointmentId && a.ClientId == clientId);
                var appointment = appointments.FirstOrDefault();

                // Запись не найдена или не принадлежит клиенту — Warning
                if (appointment == null)
                {
                    CatchException(
                        new Exception($"Запись Id={appointmentId} не найдена для клиента Id={clientId}"),
                        "ProfileController/Cancel — запись не найдена",
                        ErrorLevel.Warning);
                    TempData["CancelError"] = "Запись не найдена";
                    return RedirectToAction("Index");
                }

                // Нельзя отменить завершённую или уже отменённую запись — Warning
                if (appointment.Status != AppointmentStatus.Scheduled &&
                    appointment.Status != AppointmentStatus.Confirmed)
                {
                    TempData["CancelError"] = "Эту запись нельзя отменить";
                    return RedirectToAction("Index");
                }

                appointment.Status = AppointmentStatus.Cancelled;
                _appointmentRepo.Update(appointment);
                await _appointmentRepo.SaveChangesAsync();

                TempData["CancelSuccess"] = "Запись успешно отменена";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                // Неожиданная ошибка при отмене — Error
                CatchException(ex, "ProfileController/Cancel", ErrorLevel.Error);
                TempData["CancelError"] = "Ошибка при отмене записи. Попробуйте ещё раз.";
                return RedirectToAction("Index");
            }
        }

        [HttpGet]
        public async Task<IActionResult> Edit()
        {
            try
            {
                InitRequestInfo();

                var clientId = HttpContext.Session.GetInt32("ClientId")!.Value;
                var client = await _clientRepo.GetByIdAsync(clientId);

                if (client == null)
                {
                    return RedirectToAction("Index");
                }

                ViewBag.ClientName = client.Name;
                ViewBag.ClientPhone = client.Phone;
                ViewBag.ClientEmail = client.Email ?? "";

                return View();
            }
            catch (Exception ex)
            {
                CatchException(ex, "ProfileController/Edit GET", ErrorLevel.Error);
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Edit(string name, string? email, string? currentPassword, string? newPassword)
        {
            try
            {
                InitRequestInfo();

                var clientId = HttpContext.Session.GetInt32("ClientId")!.Value;
                var client = await _clientRepo.GetByIdAsync(clientId);

                if (client == null)
                {
                    return RedirectToAction("Index");
                }

                // Валидация имени
                if (string.IsNullOrWhiteSpace(name))
                {
                    ViewBag.EditError = "Имя не может быть пустым";
                    ViewBag.ClientName = name;
                    ViewBag.ClientPhone = client.Phone;
                    ViewBag.ClientEmail = email ?? "";
                    return View();
                }

                // Смена пароля (только если заполнили оба поля)
                if (!string.IsNullOrWhiteSpace(newPassword))
                {
                    if (string.IsNullOrWhiteSpace(currentPassword))
                    {
                        ViewBag.EditError = "Введите текущий пароль";
                        ViewBag.ClientName = name;
                        ViewBag.ClientPhone = client.Phone;
                        ViewBag.ClientEmail = email ?? "";
                        return View();
                    }

                    if (!PasswordHasher.Verify(currentPassword, client.Password ?? ""))
                        {
                        ViewBag.EditError = "Неверный текущий пароль";
                        ViewBag.ClientName = name;
                        ViewBag.ClientPhone = client.Phone;
                        ViewBag.ClientEmail = email ?? "";
                        return View();
                    }

                    client.Password = PasswordHasher.Hash(newPassword);
                }

                // Обновляем данные
                client.Name = name.Trim();
                client.Email = string.IsNullOrWhiteSpace(email) ? null : email.Trim();

                _clientRepo.Update(client);
                await _clientRepo.SaveChangesAsync();

                // Обновляем имя в сессии, чтобы сразу отобразилось
                HttpContext.Session.SetString("ClientName", client.Name);

                TempData["EditSuccess"] = "Профиль обновлён";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                CatchException(ex, "ProfileController/Edit POST", ErrorLevel.Error);
                TempData["EditError"] = "Ошибка при сохранении. Попробуйте ещё раз.";
                return RedirectToAction("Edit");
            }
        }
    }
}
