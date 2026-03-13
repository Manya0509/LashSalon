using Microsoft.AspNetCore.Mvc;
using LashBooking.Domain.Entities;
using LashBooking.Domain.Interfaces;
using LashBooking.Web.MVC.Filters;

namespace LashBooking.Web.MVC.Controllers
{
    [RequireClientAuth] // ← одна строка вместо проверки в каждом методе
    public class ProfileController : Controller
    {
        private readonly IRepository<Appointment> _appointmentRepo;
        private readonly IRepository<Service> _serviceRepo;

        public ProfileController(
            IRepository<Appointment> appointmentRepo,
            IRepository<Service> serviceRepo)
        {
            _appointmentRepo = appointmentRepo;
            _serviceRepo = serviceRepo;
        }

        public async Task<IActionResult> Index()
        {
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

        [HttpPost]
        public async Task<IActionResult> Cancel(int appointmentId)
        {
            var clientId = HttpContext.Session.GetInt32("ClientId")!.Value;

            try
            {
                var appointments = await _appointmentRepo.FindAsync(a =>
                    a.Id == appointmentId && a.ClientId == clientId);
                var appointment = appointments.FirstOrDefault();

                if (appointment == null)
                {
                    TempData["CancelError"] = "Запись не найдена";
                    return RedirectToAction("Index");
                }

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
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при отмене записи: {ex.Message}");
                TempData["CancelError"] = "Ошибка при отмене записи";
            }

            return RedirectToAction("Index");
        }
    }
}
