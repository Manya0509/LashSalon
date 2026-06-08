using LashBooking.Domain.Entities;
using LashBooking.Domain.Interfaces;
using LashBooking.Web.MVC.Services;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace LashBooking.Tests
{
    public class BookingServiceTests
    {
        private readonly Mock<IRepository<Service>> _serviceRepo;
        private readonly Mock<IRepository<Client>> _clientRepo;
        private readonly Mock<IRepository<Appointment>> _appointmentRepo;
        private readonly Mock<IRepository<BlockedSlot>> _blockedSlotRepo;
        private readonly BookingService _service;

        public BookingServiceTests()
        {

            _serviceRepo = new Mock<IRepository<Service>>();
            _clientRepo = new Mock<IRepository<Client>>();
            _appointmentRepo = new Mock<IRepository<Appointment>>();
            _blockedSlotRepo = new Mock<IRepository<BlockedSlot>>();

            _service = new BookingService(
                            _serviceRepo.Object,
                            _clientRepo.Object,
                            _appointmentRepo.Object,
                            _blockedSlotRepo.Object);
        }

        [Fact]
        public async Task CreateBookingAsync_PastTime_ReturnsFail()
        {
            var pastTime = DateTime.Now.AddDays(-1).ToString("o");

            var result = await _service.CreateBookingAsync(1, pastTime, "Иван", "+79001234567", null);

            Assert.False(result.Success);
        }

        [Fact]
        public async Task CreateBookingAsync_ServiceNotFound_ReturnsFail()
        {
            var futureTime = DateTime.Now.AddDays(1).ToString("o");

            _serviceRepo
                .Setup(r => r.GetAllAsync())
                .ReturnsAsync(new List<Service>());

            var result = await _service.CreateBookingAsync(1, futureTime, "Иван", "+79001234567", null);

            Assert.False(result.Success);
            Assert.Equal("Услуга не найдена. Обновите страницу.", result.Message);
        }

        [Fact]
        public async Task CreateBookingAsync_DayBlocked_ReturnsFail()
        {
            var futureTime = DateTime.Today.AddDays(1).AddHours(10).ToString("o");

            _serviceRepo
                .Setup(r => r.GetAllAsync())
                .ReturnsAsync(new List<Service>
                {
                    new Service { Id = 1, Name = "Ламинирование", DurationMinutes = 60 }
                });

            _blockedSlotRepo
                .Setup(r => r.FindAsync(It.IsAny<Expression<Func<BlockedSlot, bool>>>()))
                .ReturnsAsync(new List<BlockedSlot>
                {
                    new BlockedSlot { BlockedHour = null }
                });

            var result = await _service.CreateBookingAsync(1, futureTime, "Иван", "+79001234567", null);

            Assert.False(result.Success);
            Assert.Equal("Этот день недоступен для записи.", result.Message);
        }

        [Fact]
        public async Task CreateBookingAsync_HourBlocked_ReturnsFail()
        {
            var futureTime = DateTime.Today.AddDays(1).AddHours(10).ToString("o");

            _serviceRepo
                .Setup(r => r.GetAllAsync())
                .ReturnsAsync(new List<Service>
                {
                    new Service { Id = 1, Name = "Ламинирование", DurationMinutes = 60 }
                });

            _blockedSlotRepo
                .Setup(r => r.FindAsync(It.IsAny<Expression<Func<BlockedSlot, bool>>>()))
                .ReturnsAsync(new List<BlockedSlot>
                {
                    new BlockedSlot { BlockedHour = 10 }
                });

            var result = await _service.CreateBookingAsync(1, futureTime, "Иван", "+79001234567", null);

            Assert.False(result.Success);
            Assert.Equal("Выбранное время недоступно для записи.", result.Message);
        }

        [Fact]
        public async Task CreateBookingAsync_Duplicate_ReturnsFail()
        {
            var futureTime = DateTime.Today.AddDays(1).AddHours(10).ToString("o");

            _serviceRepo
                .Setup(r => r.GetAllAsync())
                .ReturnsAsync(new List<Service>
                {
                    new Service { Id = 1, Name = "Ламинирование", DurationMinutes = 60 }
                });

            _blockedSlotRepo
                .Setup(r => r.FindAsync(It.IsAny<Expression<Func<BlockedSlot, bool>>>()))
                .ReturnsAsync(new List<BlockedSlot>());

            _appointmentRepo
                .Setup(r => r.FindAsync(It.IsAny<Expression<Func<Appointment, bool>>>()))
                .ReturnsAsync(new List<Appointment>
                {
                    new Appointment { Id = 1 }
                });

            var result = await _service.CreateBookingAsync(1, futureTime, "Иван", "+79001234567", null);

            Assert.False(result.Success);
            Assert.Equal("У вас уже есть запись на этот день", result.Message);
        }

        [Fact]
        public async Task CreateBookingAsync_ValidData_ReturnsSuccess()
        {
            var futureTime = DateTime.Today.AddDays(1).AddHours(10).ToString("o");

            _serviceRepo
                .Setup(r => r.GetAllAsync())
                .ReturnsAsync(new List<Service>
                {
                    new Service { Id = 1, Name = "Ламинирование", DurationMinutes = 60 }
                });

            _blockedSlotRepo
                .Setup(r => r.FindAsync(It.IsAny<Expression<Func<BlockedSlot, bool>>>()))
                .ReturnsAsync(new List<BlockedSlot>());

            _appointmentRepo
                .Setup(r => r.FindAsync(It.IsAny<Expression<Func<Appointment, bool>>>()))
                .ReturnsAsync(new List<Appointment>());

            _clientRepo
                .Setup(r => r.FindAsync(It.IsAny<Expression<Func<Client, bool>>>()))
                .ReturnsAsync(new List<Client>());

            var result = await _service.CreateBookingAsync(1, futureTime, "Иван", "+79001234567", null);

            Assert.True(result.Success);
        }
    }
}
