using LashBooking.Domain.Entities;
using LashBooking.Domain.Interfaces;
using LashBooking.Web.MVC.Services;
using Moq;
using System.Linq.Expressions;

namespace LashBooking.Tests
{
    public class ScheduleServiceTests
    {
        private readonly Mock<IRepository<Appointment>> _appointmentRepo;
        private readonly Mock<IRepository<BlockedSlot>> _blockedSlotRepo;
        private readonly Mock<IRepository<Service>> _serviceRepo;
        private readonly ScheduleService _service;

        public ScheduleServiceTests() // конструктор — запускается перед каждым тестом
        {
            _serviceRepo = new Mock<IRepository<Service>>();
            _appointmentRepo = new Mock<IRepository<Appointment>>();
            _blockedSlotRepo = new Mock<IRepository<BlockedSlot>>();

            _service = new ScheduleService(
                _serviceRepo.Object,
                _appointmentRepo.Object,
                _blockedSlotRepo.Object);
        }

        [Fact]
        public async Task GetDayStateAsync_WeekdayNoAppointments_ReturnsFree()
        {
            // Говорим мокам что вернуть при вызове FindAsync
            _appointmentRepo
                .Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Appointment, bool>>>()))
                .ReturnsAsync(new List<Appointment>());  // пустой список — нет записей

            _blockedSlotRepo
                .Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<BlockedSlot, bool>>>()))
                .ReturnsAsync(new List<BlockedSlot>());  // пустой список — нет блокировок

            // Берём ближайший понедельник, чтобы точно не выходной
            var today = DateTime.Today;
            var daysUntilMonday = ((int)DayOfWeek.Monday - (int)today.DayOfWeek + 7) % 7;
            if (daysUntilMonday == 0) daysUntilMonday = 7;
            var monday = today.AddDays(daysUntilMonday);

            // Act
            var result = await _service.GetDayStateAsync(monday);

            // Assert
            Assert.Equal("free", result);
        }

        [Fact]
        public async Task GetDayStateAsync_Saturday_ReturnsWeekend_AndRepoNotCalled()
        {
            var today = DateTime.Today;
            var daysUntilSaturday = ((int)DayOfWeek.Saturday - (int)today.DayOfWeek + 7) % 7;
            if (daysUntilSaturday == 0) daysUntilSaturday = 7;
            var saturday = today.AddDays(daysUntilSaturday);

            var result = await _service.GetDayStateAsync(saturday);

            Assert.Equal("weekend", result);

            // Verify — проверяем что репозиторий вообще не вызывался
            _blockedSlotRepo.Verify(
                r => r.FindAsync(It.IsAny<Expression<Func<BlockedSlot, bool>>>()),
                Times.Never);
        }

        [Fact]
        public async Task GetDayStateAsync_WeekdayNoAppointments_ReturnsFreeAndRepoCalledOnce()
        {
            _appointmentRepo
                .Setup(r => r.FindAsync(It.IsAny<Expression<Func<Appointment, bool>>>()))
                .ReturnsAsync(new List<Appointment>());

            _blockedSlotRepo
                .Setup(r => r.FindAsync(It.IsAny<Expression<Func<BlockedSlot, bool>>>()))
                .ReturnsAsync(new List<BlockedSlot>());

            var today = DateTime.Today;
            var daysUntilMonday = ((int)DayOfWeek.Monday - (int)today.DayOfWeek + 7) % 7;
            if (daysUntilMonday == 0) daysUntilMonday = 7;
            var monday = today.AddDays(daysUntilMonday);

            // Act
            var result = await _service.GetDayStateAsync(monday);

            // Assert
            Assert.Equal("free", result);

            _appointmentRepo.Verify(
                r => r.FindAsync(It.IsAny<Expression<Func<Appointment, bool>>>()),
                Times.Once);
        }

        [Fact]
        public async Task GetDayStateAsync_PastDate_ReturnsPast()
        {
            var date = DateTime.Today.AddDays(-1);

            var result = await _service.GetDayStateAsync(date);

            _blockedSlotRepo.Verify(
                r => r.FindAsync(It.IsAny<Expression<Func<BlockedSlot, bool>>>()),
                Times.Never);

            Assert.Equal("past", result);
        }

        [Fact]
        public async Task GetDayStateAsync_FullDate_ReturnsFull()
        {
            var date = DateTime.Today.AddDays(1);

            _blockedSlotRepo
                .Setup(r => r.FindAsync(It.IsAny<Expression<Func<BlockedSlot, bool>>>()))
                .ReturnsAsync(new List<BlockedSlot>
                {
                    new BlockedSlot { BlockedHour = null }
                });

            var result = await _service.GetDayStateAsync(date);

            Assert.Equal("full", result);
        }

        [Fact]
        public async Task GetDayStateAsync_PartialDate_ReturnsPartial()
        {
            var date = DateTime.Today.AddDays(1);

            _appointmentRepo
                 .Setup(r => r.FindAsync(It.IsAny<Expression<Func<Appointment, bool>>>()))
                 .ReturnsAsync(new List<Appointment>
                 {
                    new Appointment
                    {
                      DateStart = date.AddHours(10),
                      DateEnd = date.AddHours(11)
                    }
                 });

            _blockedSlotRepo
                .Setup(r => r.FindAsync(It.IsAny<Expression<Func<BlockedSlot, bool>>>()))
                .ReturnsAsync(new List<BlockedSlot>());

            var result = await _service.GetDayStateAsync(date);

            Assert.Equal("partial", result);
        }

        [Fact]
        public async Task GetFreeSlotsForDateAsync_FreeDate_ReturnsSlots()
        {
            var date = DateTime.Today.AddDays(-1);

            var result = await _service.GetFreeSlotsForDateAsync(date);

            _blockedSlotRepo.Verify(
                r => r.FindAsync(It.IsAny<Expression<Func<BlockedSlot, bool>>>()),
                Times.Never);

            Assert.Empty(result);
        }

        [Fact]
        public async Task GetFreeSlotsForDateAsync_Weekend_ReturnsEmpty()
        {
            var today = DateTime.Today;
            var daysUntilSaturday = ((int)DayOfWeek.Saturday - (int)today.DayOfWeek + 7) % 7;
            if (daysUntilSaturday == 0) daysUntilSaturday = 7;
            var saturday = today.AddDays(daysUntilSaturday);

            var result = await _service.GetFreeSlotsForDateAsync(saturday);

            _blockedSlotRepo.Verify(
                r => r.FindAsync(It.IsAny<Expression<Func<BlockedSlot, bool>>>()),
                Times.Never);

            Assert.Empty(result);
        }

        [Fact]
        public async Task GetFreeSlotsForDateAsync_FullDate_AndRepoNotCalled()
        {
            var date = DateTime.Today.AddDays(1);

            _blockedSlotRepo
                .Setup(r => r.FindAsync(It.IsAny<Expression<Func<BlockedSlot, bool>>>()))
                .ReturnsAsync(new List<BlockedSlot>
                {
                    new BlockedSlot { BlockedHour = null }
                });

            var result = await _service.GetFreeSlotsForDateAsync(date);

            Assert.Empty(result);
        }

        [Fact]
        public async Task GetFreeSlotsForDateAsync_FreeDate_AndRepoNotCalled()
        {
            var date = DateTime.Today.AddDays(1);

            _blockedSlotRepo
                .Setup(r => r.FindAsync(It.IsAny<Expression<Func<BlockedSlot, bool>>>()))
                .ReturnsAsync(new List<BlockedSlot>());

            _appointmentRepo
                .Setup(r => r.FindAsync(It.IsAny<Expression<Func<Appointment, bool>>>()))
                .ReturnsAsync(new List<Appointment>());

            var result = await _service.GetFreeSlotsForDateAsync(date);

            Assert.Equal(9, result.Count);
        }

        [Fact]
        public async Task GetSlotsAsync_Weekend_ReturnsEmpty()
        {
            var today = DateTime.Today;
            var daysUntilSaturday = ((int)DayOfWeek.Saturday - (int)today.DayOfWeek + 7) % 7;
            if (daysUntilSaturday == 0) daysUntilSaturday = 7;
            var saturday = today.AddDays(daysUntilSaturday);

            _blockedSlotRepo
                .Setup(r => r.FindAsync(It.IsAny<Expression<Func<BlockedSlot, bool>>>()))
                .ReturnsAsync(new List<BlockedSlot>());

            _serviceRepo
                .Setup(r => r.GetAllAsync())
                .ReturnsAsync(new List<Service>
                {
                    new Service { Id = 1, Name = "Ламинирование", DurationMinutes = 60, IsActive = true }
                });

            var result = await _service.GetSlotsAsync(saturday, 1);

            Assert.Empty(result);
        }

        [Fact]
        public async Task GetSlotsAsync_NoService_AndRepoNotCalled()
        {
            var today = DateTime.Today;
            
            _blockedSlotRepo
                .Setup(r => r.FindAsync(It.IsAny<Expression<Func<BlockedSlot, bool>>>()))
                .ReturnsAsync(new List<BlockedSlot>());

            _serviceRepo
                .Setup(r => r.GetAllAsync())
                .ReturnsAsync(new List<Service>());

            var result = await _service.GetSlotsAsync(today, 1);

            Assert.Empty(result);
        }

        [Fact]
        public async Task GetSlotsAsync_DayBlock_AndRepoNotCalled()
        {
            var today = DateTime.Today;

            _blockedSlotRepo
                .Setup(r => r.FindAsync(It.IsAny<Expression<Func<BlockedSlot, bool>>>()))
                .ReturnsAsync(new List<BlockedSlot>
                {
                    new BlockedSlot { BlockedHour = null }
                });

            _serviceRepo
                .Setup(r => r.GetAllAsync())
                .ReturnsAsync(new List<Service>
                {
                    new Service { Id = 1, Name = "Ламинирование", DurationMinutes = 60, IsActive = true }
                });

            var result = await _service.GetSlotsAsync(today, 1);

            Assert.Empty(result);
        }

        [Fact]
        public async Task GetSlotsAsync_NoServiceAndBlock_AndRepoNotCalled()
        {
            var today = DateTime.Today;

            _blockedSlotRepo
                .Setup(r => r.FindAsync(It.IsAny<Expression<Func<BlockedSlot, bool>>>()))
                .ReturnsAsync(new List<BlockedSlot>());

            _appointmentRepo
                .Setup(r => r.FindAsync(It.IsAny<Expression<Func<Appointment, bool>>>()))
                .ReturnsAsync(new List<Appointment>());

            _serviceRepo
                .Setup(r => r.GetAllAsync())
                .ReturnsAsync(new List<Service>
                {
                    new Service { Id = 1, Name = "Ламинирование", DurationMinutes = 60, IsActive = true }
                });

            var result = await _service.GetSlotsAsync(today, 1);

            Assert.All(result, slot => Assert.False(slot.IsBusy));
        }

        [Fact]
        public async Task GetSlotsAsync_OneSlotBusy_AndRepoNotCalled()
        {
            var today = DateTime.Today;

            _blockedSlotRepo
                .Setup(r => r.FindAsync(It.IsAny<Expression<Func<BlockedSlot, bool>>>()))
                .ReturnsAsync(new List<BlockedSlot>());

            _appointmentRepo
                .Setup(r => r.FindAsync(It.IsAny<Expression<Func<Appointment, bool>>>()))
                .ReturnsAsync(new List<Appointment>
                {
                    new Appointment
                    {
                    DateStart = today.AddHours(10),
                    DateEnd = today.AddHours(11)
                    }
                });

            _serviceRepo
                .Setup(r => r.GetAllAsync())
                .ReturnsAsync(new List<Service>
                {
                    new Service { Id = 1, Name = "Ламинирование", DurationMinutes = 60, IsActive = true }
                });

            var result = await _service.GetSlotsAsync(today, 1);

            Assert.Contains(result, slot => slot.IsBusy);
        }

        [Fact]
        public async Task GetAvailableDatesAsync_NoService_AndRepoNotCalled()
        {
            var today = DateTime.Today;

            _serviceRepo
                .Setup(r => r.GetAllAsync())
                .ReturnsAsync(new List<Service>());

            var result = await _service.GetAvailableDatesAsync(1);

            Assert.Empty(result);
        }

        [Fact]
        public async Task GetAvailableDatesAsync_NoBlockAndService_AndRepoNotCalled()
        {
            var today = DateTime.Today;

            _blockedSlotRepo
                .Setup(r => r.GetAllAsync())
                .ReturnsAsync(new List<BlockedSlot>());

            _appointmentRepo
                .Setup(r => r.FindAsync(It.IsAny<Expression<Func<Appointment, bool>>>()))
                .ReturnsAsync(new List<Appointment>());

            _serviceRepo
                .Setup(r => r.GetAllAsync())
                .ReturnsAsync(new List<Service>
                {
                    new Service { Id = 1, Name = "Ламинирование", DurationMinutes = 60, IsActive = true }
                });

            var result = await _service.GetAvailableDatesAsync(1);

            Assert.NotEmpty(result);
        }
    }
}