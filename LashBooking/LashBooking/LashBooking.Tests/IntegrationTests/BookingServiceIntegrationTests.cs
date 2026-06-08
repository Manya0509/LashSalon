using LashBooking.Domain.Entities;
using LashBooking.Infrastructure.Data;
using LashBooking.Infrastructure.Repositories;
using LashBooking.Web.MVC.Services;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LashBooking.Tests.IntegrationTests
{
    public class BookingServiceIntegrationTests
    {
        private ApplicationDbContext CreateDb()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase("TestDb_" + Guid.NewGuid()) 
                .Options;                                         
            return new ApplicationDbContext(options);
        }

        [Fact]
        public async Task CreateBookingAsync_ValidData_AppointmentSavedToDatabase() // запись создаётся и сохраняется в базе
        {
            var db = CreateDb();   // Создание БД в память

            // Создаем настоящие репозитории 
            var serviceRepo = new GenericRepository<Service>(db);
            var clientRepo = new GenericRepository<Client>(db);
            var appointmentRepo = new GenericRepository<Appointment>(db);
            var blockedSlotRepo = new GenericRepository<BlockedSlot>(db);

            // Добавляем услугу в тестовую базу
            await db.Services.AddAsync(new Service
            {
                Id = 1,
                Name = "Ламинирование",
                DurationMinutes = 60,
                IsActive = true
            }, TestContext.Current.CancellationToken); 

            await db.SaveChangesAsync(TestContext.Current.CancellationToken);

            var service = new BookingService(serviceRepo, clientRepo, appointmentRepo, blockedSlotRepo);
            var futureTime = DateTime.Today.AddDays(1).AddHours(10).ToString("o");

            var result = await service.CreateBookingAsync(1, futureTime, "Иван", "+79001234567", null);

            Assert.True(result.Success);

            Assert.Equal(1, db.Appointments.Count()); // Проверяем результат И состояние базы
        }

        [Fact]
        public async Task CreateBookingAsync_Duplicate_NoNewAppointmentInDatabase() // повторная запись отклоняется, в базе остаётся одна запись
        {
            var db = CreateDb(); 

            var serviceRepo = new GenericRepository<Service>(db);
            var clientRepo = new GenericRepository<Client>(db);
            var appointmentRepo = new GenericRepository<Appointment>(db);
            var blockedSlotRepo = new GenericRepository<BlockedSlot>(db);

            await db.Services.AddAsync(new Service
            {
                Id = 1,
                Name = "Ламинирование",
                DurationMinutes = 60,
                IsActive = true,
            }, TestContext.Current.CancellationToken);
            await db.SaveChangesAsync(TestContext.Current.CancellationToken);

            await db.Clients.AddAsync(new Client
            {
                Id = 1,
                Phone = "+79001234567",
                Name = "Иван"
            }, TestContext.Current.CancellationToken);
            await db.SaveChangesAsync(TestContext.Current.CancellationToken);

            await db.Appointments.AddAsync(new Appointment
            {
                Id = 1,
                DateStart = new DateTime(2026, 6, 5, 10, 0, 0),
                DateEnd = new DateTime(2026, 6, 5, 12, 0, 0),
                ClientId = 1,
            }, TestContext.Current.CancellationToken);
            await db.SaveChangesAsync(TestContext.Current.CancellationToken);

            var service = new BookingService(serviceRepo, clientRepo, appointmentRepo, blockedSlotRepo);
            var futureTime = new DateTime(2026, 6, 5, 11, 0, 0).ToString("o");

            var result = await service.CreateBookingAsync(1, futureTime, "Иван", "+79001234567", null);

            Assert.False(result.Success);

            Assert.Equal(1, db.Appointments.Count()); 
        }

        [Fact]
        public async Task ValidData_NewClientCreatedInDatabase() // при успешной записи новый клиент появился в базе
        {
            var db = CreateDb();  

            var serviceRepo = new GenericRepository<Service>(db);
            var clientRepo = new GenericRepository<Client>(db);
            var appointmentRepo = new GenericRepository<Appointment>(db);
            var blockedSlotRepo = new GenericRepository<BlockedSlot>(db);

            await db.Services.AddAsync(new Service
            {
                Id = 1,
                Name = "Ламинирование",
                DurationMinutes = 60,
                IsActive = true
            }, TestContext.Current.CancellationToken);

            await db.SaveChangesAsync(TestContext.Current.CancellationToken);

            var service = new BookingService(serviceRepo, clientRepo, appointmentRepo, blockedSlotRepo);
            var futureTime = DateTime.Today.AddDays(1).AddHours(10).ToString("o");

            var result = await service.CreateBookingAsync(1, futureTime, "Иван", "+79001234567", null);

            Assert.True(result.Success);

            Assert.Equal(1, db.Clients.Count()); 
        }

        [Fact]
        public async Task DayBlocked_NoAppointmentInDatabase() // Заблокированный день — запись не создалась
        {
            var db = CreateDb();  

            var serviceRepo = new GenericRepository<Service>(db);
            var clientRepo = new GenericRepository<Client>(db);
            var appointmentRepo = new GenericRepository<Appointment>(db);
            var blockedSlotRepo = new GenericRepository<BlockedSlot>(db);

            await db.Services.AddAsync(new Service
            {
                Id = 1,
                Name = "Ламинирование",
                DurationMinutes = 60,
                IsActive = true
            }, TestContext.Current.CancellationToken);
            await db.SaveChangesAsync(TestContext.Current.CancellationToken);

            await db.BlockedSlots.AddAsync(new BlockedSlot
            {
                BlockedHour = null,
                Date = DateTime.Today.AddDays(1)
            }, TestContext.Current.CancellationToken);
            await db.SaveChangesAsync(TestContext.Current.CancellationToken);

            var service = new BookingService(serviceRepo, clientRepo, appointmentRepo, blockedSlotRepo);
            var futureTime = DateTime.Today.AddDays(1).AddHours(10).ToString("o");

            var result = await service.CreateBookingAsync(1, futureTime, "Иван", "+79001234567", null);

            Assert.False(result.Success);

            Assert.Equal(0, db.Appointments.Count());
        }

        [Fact]
        public async Task ValidData_ExistingClientReused() // Клиент есть в базе - запись не задублировалась
        {
            var db = CreateDb();  

            var serviceRepo = new GenericRepository<Service>(db);
            var clientRepo = new GenericRepository<Client>(db);
            var appointmentRepo = new GenericRepository<Appointment>(db);
            var blockedSlotRepo = new GenericRepository<BlockedSlot>(db);

            await db.Services.AddAsync(new Service
            {
                Id = 1,
                Name = "Ламинирование",
                DurationMinutes = 60,
                IsActive = true
            }, TestContext.Current.CancellationToken);
            await db.SaveChangesAsync(TestContext.Current.CancellationToken);

            await db.Clients.AddAsync(new Client
            {
                Id = 1,
                Phone = "+79001234567",
                Name = "Иван"
            }, TestContext.Current.CancellationToken);
            await db.SaveChangesAsync(TestContext.Current.CancellationToken);

            var service = new BookingService(serviceRepo, clientRepo, appointmentRepo, blockedSlotRepo);
            var futureTime = DateTime.Today.AddDays(1).AddHours(10).ToString("o");

            var result = await service.CreateBookingAsync(1, futureTime, "Иван", "+79001234567", null);

            Assert.True(result.Success);

            Assert.Equal(1, db.Clients.Count()); 
        }

        [Fact]
        public async Task HourBlocked_NoAppointmentInDatabase() // Конкретный час заблокирован
        {
            var db = CreateDb();  

            var serviceRepo = new GenericRepository<Service>(db);
            var clientRepo = new GenericRepository<Client>(db);
            var appointmentRepo = new GenericRepository<Appointment>(db);
            var blockedSlotRepo = new GenericRepository<BlockedSlot>(db);

            await db.Services.AddAsync(new Service
            {
                Id = 1,
                Name = "Ламинирование",
                DurationMinutes = 60,
                IsActive = true
            }, TestContext.Current.CancellationToken);
            await db.SaveChangesAsync(TestContext.Current.CancellationToken);

            await db.BlockedSlots.AddAsync(new BlockedSlot
            {
                BlockedHour = 10,
                Date = DateTime.Today.AddDays(1)
            }, TestContext.Current.CancellationToken);
            await db.SaveChangesAsync(TestContext.Current.CancellationToken);

            var service = new BookingService(serviceRepo, clientRepo, appointmentRepo, blockedSlotRepo);
            var futureTime = DateTime.Today.AddDays(1).AddHours(10).ToString("o");

            var result = await service.CreateBookingAsync(1, futureTime, "Иван", "+79001234567", null);

            Assert.False(result.Success);

            Assert.Equal(0, db.Appointments.Count()); 
        }

        [Fact]
        public async Task ServiceNotFound_NoAppointmentInDatabase() // Услуга не найдена
        {
            var db = CreateDb(); 

            var serviceRepo = new GenericRepository<Service>(db);
            var clientRepo = new GenericRepository<Client>(db);
            var appointmentRepo = new GenericRepository<Appointment>(db);
            var blockedSlotRepo = new GenericRepository<BlockedSlot>(db);

            var service = new BookingService(serviceRepo, clientRepo, appointmentRepo, blockedSlotRepo);
            var futureTime = DateTime.Today.AddDays(1).AddHours(10).ToString("o");

            var result = await service.CreateBookingAsync(1, futureTime, "Иван", "+79001234567", null);

            Assert.False(result.Success);

            Assert.Equal(0, db.Appointments.Count()); 
        }
    }
}
