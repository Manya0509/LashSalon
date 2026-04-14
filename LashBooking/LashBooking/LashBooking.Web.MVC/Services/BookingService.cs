using LashBooking.Domain.Constants;
using LashBooking.Domain.Entities;
using LashBooking.Domain.Interfaces;
using LashBooking.Domain.Models;

namespace LashBooking.Web.MVC.Services
{
    // Реализация IBookingService.
    public class BookingService : IBookingService
    {
        private readonly IRepository<Service> _serviceRepo;
        private readonly IRepository<Client> _clientRepo;
        private readonly IRepository<Appointment> _appointmentRepo;
        private readonly IRepository<BlockedSlot> _blockedSlotRepo;

        public BookingService(
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

        // Создать новую запись со всеми проверками.
        public async Task<ServiceResult> CreateBookingAsync(
            int serviceId,
            string selectedTime,
            string clientName,
            string clientPhone,
            int? clientId)
        {
            // === ПРОВЕРКА 1: Время ===
            // Парсим строку "2026-03-28T10:00:00" в DateTime.
            // Если строка кривая или время в прошлом — отказ.
            if (!DateTime.TryParse(selectedTime, out DateTime time) || time < DateTime.Now)
                return ServiceResult.Fail("Пожалуйста, выберите доступное время");

            // === ПРОВЕРКА 2: Услуга ===
            // Ищем услугу по Id. Если не нашли — значит страница устарела.
            var service = (await _serviceRepo.GetAllAsync())
                .FirstOrDefault(x => x.Id == serviceId);

            if (service == null)
                return ServiceResult.Fail("Услуга не найдена. Обновите страницу.");

            // === ПРОВЕРКА 3: Влезает в рабочий день? ===
            // Если услуга 90 минут, а выбрано 17:00 — конец в 18:30,
            // а рабочий день до 18:00. Не влезает.
            var dateEnd = time.AddMinutes(service.DurationMinutes);
            var workDayEnd = time.Date.AddHours(WorkSchedule.EndHour);

            if (dateEnd > workDayEnd)
                return ServiceResult.Fail(
                    $"Недостаточно времени для услуги «{service.Name}». " +
                    $"Выберите более раннее время.");

            // === ПРОВЕРКА 4-5: Блокировки ===
            // Загружаем все блокировки на этот день.
            var blockedSlots = (await _blockedSlotRepo
                .FindAsync(b => b.Date.Date == time.Date)).ToList();

            // BlockedHour == null — значит заблокирован ВЕСЬ день
            if (blockedSlots.Any(b => b.BlockedHour == null))
                return ServiceResult.Fail("Этот день недоступен для записи.");

            // Конкретный час заблокирован
            if (blockedSlots.Any(b => b.BlockedHour == time.Hour))
                return ServiceResult.Fail("Выбранное время недоступно для записи.");

            // === ПРОВЕРКА 6: Дубликат ===
            // Один клиент — одна запись в день.
            // Ищем по телефону (не по Id), потому что клиент может быть не авторизован.
            var existing = await _appointmentRepo.FindAsync(a =>
                a.Client != null &&
                a.Client.Phone == clientPhone &&
                a.DateStart.Date == time.Date);

            if (existing.Any())
                return ServiceResult.Fail("У вас уже есть запись на этот день");

            // === ШАГИ 7: Поиск или создание клиента ===
            // Приоритет:
            // 1. Если клиент авторизован (clientId не null) — ищем по Id
            // 2. Если не нашли по Id — ищем по телефону
            // 3. Если вообще не нашли — создаём нового
            Client? client = null;

            if (clientId.HasValue)
            {
                var clients = await _clientRepo
                    .FindAsync(c => c.Id == clientId.Value);
                client = clients.FirstOrDefault();
            }

            if (client == null)
            {
                var clients = await _clientRepo
                    .FindAsync(c => c.Phone == clientPhone);
                client = clients.FirstOrDefault();
            }

            if (client == null)
            {
                client = new Client
                {
                    Name = clientName,
                    Phone = clientPhone,
                    CreatedAt = DateTime.Now
                };
                await _clientRepo.AddAsync(client);
                await _clientRepo.SaveChangesAsync();
            }

            // === ШАГ 8: Создание записи ===
            var appointment = new Appointment
            {
                ClientId = client.Id,
                ServiceId = serviceId,
                DateStart = time,
                DateEnd = dateEnd,
                CreatedAt = DateTime.Now
            };

            await _appointmentRepo.AddAsync(appointment);
            await _appointmentRepo.SaveChangesAsync();

            // Всё прошло — возвращаем успех
            return ServiceResult.Ok(
    $"Вы успешно записаны! {service.Name} — {time:dd.MM.yyyy HH:mm}");

        }
    }
}
