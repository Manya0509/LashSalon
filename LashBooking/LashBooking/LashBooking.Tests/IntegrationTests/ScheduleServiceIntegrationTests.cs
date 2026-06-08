using DevExpress.XtraPrinting.Native;
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
    public class ScheduleServiceIntegrationTests
    {
        private ApplicationDbContext CreateDb()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase("TestDb_" + Guid.NewGuid())
                .Options;
            return new ApplicationDbContext(options);
        }

        [Fact]
        public async Task GetDayStateAsync_FreeDay_ReturnsFree() // Будний день - пустая база
        {
            var db = CreateDb();

            var serviceRepo = new GenericRepository<Service>(db);
            var appointmentRepo = new GenericRepository<Appointment>(db);
            var blockedSlotRepo = new GenericRepository<BlockedSlot>(db);

            var service = new ScheduleService(serviceRepo, appointmentRepo, blockedSlotRepo);
            var today = DateTime.Today;
            var daysUntilMonday = ((int)DayOfWeek.Monday - (int)today.DayOfWeek + 7) % 7;
            if (daysUntilMonday == 0) daysUntilMonday = 7;
            var monday = today.AddDays(daysUntilMonday);

            var result = await service.GetDayStateAsync(monday);

            Assert.Equal("free", result);
        }

        [Fact]
        public async Task GetDayStateAsync_WithAppointment_ReturnsPartial() // 	Одна запись на этот день
        {
            var db = CreateDb();

            var serviceRepo = new GenericRepository<Service>(db);
            var appointmentRepo = new GenericRepository<Appointment>(db);
            var blockedSlotRepo = new GenericRepository<BlockedSlot>(db);

            var today = DateTime.Today;
            var daysUntilMonday = ((int)DayOfWeek.Monday - (int)today.DayOfWeek + 7) % 7;
            if (daysUntilMonday == 0) daysUntilMonday = 7;
            var monday = today.AddDays(daysUntilMonday);

            await db.Appointments.AddAsync(new Appointment
            {
                DateStart = monday.AddHours(10),
                DateEnd = monday.AddHours(11),
                ClientId = 1,
            }, TestContext.Current.CancellationToken);
            await db.SaveChangesAsync(TestContext.Current.CancellationToken);

            var service = new ScheduleService(serviceRepo, appointmentRepo, blockedSlotRepo);
            var result = await service.GetDayStateAsync(monday);

            Assert.Equal("partial", result);
        }

        [Fact]
        public async Task GetDayStateAsync_DayBlocked_ReturnsFull() // день заблокирован администратором
        {
            var db = CreateDb();

            var serviceRepo = new GenericRepository<Service>(db);
            var appointmentRepo = new GenericRepository<Appointment>(db);
            var blockedSlotRepo = new GenericRepository<BlockedSlot>(db);

            var today = DateTime.Today;
            var daysUntilMonday = ((int)DayOfWeek.Monday - (int)today.DayOfWeek + 7) % 7;
            if (daysUntilMonday == 0) daysUntilMonday = 7;
            var monday = today.AddDays(daysUntilMonday);

            await db.BlockedSlots.AddAsync(new BlockedSlot
            {
                Date = monday,
                BlockedHour = null
            }, TestContext.Current.CancellationToken);
            await db.SaveChangesAsync(TestContext.Current.CancellationToken);

            var service = new ScheduleService(serviceRepo, appointmentRepo, blockedSlotRepo);
            var result = await service.GetDayStateAsync(monday);

            Assert.Equal("full", result);
        }

        [Fact]
        public async Task GetFreeSlotsForDateAsync_PastDate_ReturnsEmpty() // Прошедший день
        {
            var db = CreateDb();

            var serviceRepo = new GenericRepository<Service>(db);
            var appointmentRepo = new GenericRepository<Appointment>(db);
            var blockedSlotRepo = new GenericRepository<BlockedSlot>(db);

            var service = new ScheduleService(serviceRepo, appointmentRepo, blockedSlotRepo);
            var date = DateTime.Today.AddDays(-1);
            var result = await service.GetFreeSlotsForDateAsync(date);

            Assert.Empty(result);
        }

        [Fact]
        public async Task GetFreeSlotsForDateAsync_Weekend_ReturnsEmpty() // Выходной день
        {
            var db = CreateDb();

            var serviceRepo = new GenericRepository<Service>(db);
            var appointmentRepo = new GenericRepository<Appointment>(db);
            var blockedSlotRepo = new GenericRepository<BlockedSlot>(db);

            var service = new ScheduleService(serviceRepo, appointmentRepo, blockedSlotRepo);
            var today = DateTime.Today;
            var daysUntilSaturday = ((int)DayOfWeek.Saturday - (int)today.DayOfWeek + 7) % 7;
            if (daysUntilSaturday == 0) daysUntilSaturday = 7;
            var saturday = today.AddDays(daysUntilSaturday);

            var result = await service.GetFreeSlotsForDateAsync(saturday);

            Assert.Empty(result);
        }

        [Fact]
        public async Task GetFreeSlotsForDateAsync_BlockedDay_ReturnsEmpty() // Заблокированный день
        {
            var db = CreateDb();

            var serviceRepo = new GenericRepository<Service>(db);
            var appointmentRepo = new GenericRepository<Appointment>(db);
            var blockedSlotRepo = new GenericRepository<BlockedSlot>(db);

            var service = new ScheduleService(serviceRepo, appointmentRepo, blockedSlotRepo);
            var today = DateTime.Today;
            var daysUntilMonday = ((int)DayOfWeek.Monday - (int)today.DayOfWeek + 7) % 7;
            if (daysUntilMonday == 0) daysUntilMonday = 7;
            var monday = today.AddDays(daysUntilMonday);

            await db.BlockedSlots.AddAsync(new BlockedSlot
            {
                Date = monday,
                BlockedHour = null
            }, TestContext.Current.CancellationToken);
            await db.SaveChangesAsync(TestContext.Current.CancellationToken);

           var result = await service.GetFreeSlotsForDateAsync(monday);

            Assert.Empty(result);
        }

        [Fact]
        public async Task GetFreeSlotsForDateAsync_FreeDay_ReturnsSlots() // Свободный день
        {
            var db = CreateDb();

            var serviceRepo = new GenericRepository<Service>(db);
            var appointmentRepo = new GenericRepository<Appointment>(db);
            var blockedSlotRepo = new GenericRepository<BlockedSlot>(db);

            var service = new ScheduleService(serviceRepo, appointmentRepo, blockedSlotRepo);
            var today = DateTime.Today;
            var daysUntilMonday = ((int)DayOfWeek.Monday - (int)today.DayOfWeek + 7) % 7;
            if (daysUntilMonday == 0) daysUntilMonday = 7;
            var monday = today.AddDays(daysUntilMonday);

            var result = await service.GetFreeSlotsForDateAsync(monday);

            Assert.NotEmpty(result);
        }

        [Fact]
        public async Task GetSlotsAsync_NoService_ReturnsEmpty() //  Услуга не найдена → пустой список
        {
            var db = CreateDb();

            var serviceRepo = new GenericRepository<Service>(db);
            var appointmentRepo = new GenericRepository<Appointment>(db);
            var blockedSlotRepo = new GenericRepository<BlockedSlot>(db);

            var service = new ScheduleService(serviceRepo, appointmentRepo, blockedSlotRepo);
            var today = DateTime.Today;

            var result = await service.GetSlotsAsync(today, 1);

            Assert.Empty(result);
        }

        [Fact]
        public async Task GetSlotsAsync_Weekend_ReturnsEmpty() // Выходной день
        {
            var db = CreateDb();

            var serviceRepo = new GenericRepository<Service>(db);
            var appointmentRepo = new GenericRepository<Appointment>(db);
            var blockedSlotRepo = new GenericRepository<BlockedSlot>(db);

            var service = new ScheduleService(serviceRepo, appointmentRepo, blockedSlotRepo);
            var today = DateTime.Today;
            var daysUntilSaturday = ((int)DayOfWeek.Saturday - (int)today.DayOfWeek + 7) % 7;
            if (daysUntilSaturday == 0) daysUntilSaturday = 7;
            var saturday = today.AddDays(daysUntilSaturday);

            var result = await service.GetSlotsAsync(saturday, 1);

            Assert.Empty(result);
        }

        [Fact]
        public async Task GetSlotsAsync_BlockedDay_ReturnsEmpty() // Заблокированный день
        {
            var db = CreateDb();

            var serviceRepo = new GenericRepository<Service>(db);
            var appointmentRepo = new GenericRepository<Appointment>(db);
            var blockedSlotRepo = new GenericRepository<BlockedSlot>(db);

            var service = new ScheduleService(serviceRepo, appointmentRepo, blockedSlotRepo);
            var today = DateTime.Today;
            var daysUntilMonday = ((int)DayOfWeek.Monday - (int)today.DayOfWeek + 7) % 7;
            if (daysUntilMonday == 0) daysUntilMonday = 7;
            var monday = today.AddDays(daysUntilMonday);

            await db.BlockedSlots.AddAsync(new BlockedSlot
            {
                Date = monday,
                BlockedHour = null
            }, TestContext.Current.CancellationToken);
            await db.SaveChangesAsync(TestContext.Current.CancellationToken);

            var result = await service.GetSlotsAsync(monday, 1);

            Assert.Empty(result);
        }

        [Fact]
        public async Task GetAvailableDatesAsync_NoService_ReturnsEmpty() // Услуга не найдена → пустой список
        {
            var db = CreateDb();

            var serviceRepo = new GenericRepository<Service>(db);
            var appointmentRepo = new GenericRepository<Appointment>(db);
            var blockedSlotRepo = new GenericRepository<BlockedSlot>(db);

            var service = new ScheduleService(serviceRepo, appointmentRepo, blockedSlotRepo);
            var today = DateTime.Today;

            var result = await service.GetAvailableDatesAsync(1);

            Assert.Empty(result);
        }

        [Fact]
        public async Task GetAvailableDatesAsync_WithService_ReturnsAvailableDates() // Услуга есть, нет блокировок → есть доступные даты
        {
            var db = CreateDb();

            var serviceRepo = new GenericRepository<Service>(db);
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

            var service = new ScheduleService(serviceRepo, appointmentRepo, blockedSlotRepo);

            var result = await service.GetAvailableDatesAsync(1);

            Assert.NotEmpty(result);
        }
    }
}
