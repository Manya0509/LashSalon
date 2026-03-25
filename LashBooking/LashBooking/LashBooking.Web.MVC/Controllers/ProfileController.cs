using Microsoft.AspNetCore.Mvc;
using LashBooking.Domain.Entities;
using LashBooking.Domain.Interfaces;
using LashBooking.Domain.Constants;
using LashBooking.Web.MVC.Filters;

namespace LashBooking.Web.MVC.Controllers
{
    [RequireClientAuth]
    public class ProfileController : BaseController
    {
        private readonly IRepository<Appointment> _appointmentRepo;
        private readonly IRepository<Service> _serviceRepo;

        public ProfileController(
            IRepository<Appointment> appointmentRepo,
            IRepository<Service> serviceRepo,
            ILogger logger) : base(logger)
        {
            _appointmentRepo = appointmentRepo;
            _serviceRepo = serviceRepo;
        }

        public async Task<IActionResult> Index()
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
                var appointmentList = appointments
                    .OrderByDescending(a => a.DateStart)
                    .ToList();

                ViewBag.ClientName = clientName;
                ViewBag.Appointments = appointmentList;
                ViewBag.ServicesDictionary = servicesDictionary;
                ViewBag.CancelSuccess = TempData["CancelSuccess"];
                ViewBag.CancelError = TempData["CancelError"];

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
    }
}
