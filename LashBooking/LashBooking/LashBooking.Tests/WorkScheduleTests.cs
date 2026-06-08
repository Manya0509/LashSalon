using LashBooking.Domain.Constants;

namespace LashBooking.Tests
{
    public class WorkScheduleTests
    {
        [Fact]
        public void IsWeekend_Sunday_ReturnsTrue()
        {
            var sunday = new DateTime(2026, 5, 24);
            var result = WorkSchedule.IsWeekend(sunday);
            Assert.True(result);
        }

        [Fact]
        public void IsWeekend_Saturday_ReturnsTrue()
        {
            var saturday = new DateTime(2026, 5, 23);
            var result = WorkSchedule.IsWeekend(saturday);
            Assert.True(result);
        }

        [Fact]
        public void IsWeekend_Monday_ReturnsFalse()
        {
            var monday = new DateTime(2026, 5, 25);
            var result = WorkSchedule.IsWeekend(monday);
            Assert.False(result);
        }

        [Fact]
        public void IsWeekend_Friday_ReturnsFalse() // Проверка: пятница не является выходным днем    +
        {
            var friday = new DateTime(2026, 5, 15);
            var result = WorkSchedule.IsWeekend(friday);
            Assert.False(result);
        }

        [Fact]
        public void IsWithinWorkingHours_Hour9_ReturnsTrue() // Проверка: 9:00 входит в рабочие часы
        {
            var hour = 9;
            var result = WorkSchedule.IsWithinWorkingHours(hour);
            Assert.True(result);
        }

        [Fact]
        public void IsWithinWorkingHours_Hour8_ReturnsFalse() // Проверка: 8:00 не входит в рабочие часы
        {
            var hours = 8;
            var result = WorkSchedule.IsWithinWorkingHours(hours);
            Assert.False(result);
        }

        [Fact]
        public void IsWithinWorkingHours_Hour18_ReturnsFalse() // Проверка: 18:00 не входит в рабочие часы
        {
            var hours = 18;
            var result = WorkSchedule.IsWithinWorkingHours(hours);
            Assert.False(result);
        }

        [Theory]
        [InlineData("2026-05-09")] // суббота
        [InlineData("2026-05-10")] // воскресенье
        public void IsWeekend_WeekendDays_ReturnsTrue(string date)
        {
            var day = DateTime.Parse(date);
            var result = WorkSchedule.IsWeekend(day);    
            Assert.True(result);
        }

        [Theory]
        [InlineData(9)]
        [InlineData(12)]
        [InlineData(17)]
        public void IsWithinWorkingHours_HourTime_ReturnsTrue(int hour)  //+
        {
            // Act
            var result = WorkSchedule.IsWithinWorkingHours(hour);

            // Assert
            Assert.True(result);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(8)]
        [InlineData(18)]
        [InlineData(23)]
        public void IsWithinWorkingHours_HourTime_ReturnsFalse(int hour) //+
        {
            // Act
            var result = WorkSchedule.IsWithinWorkingHours(hour);

            // Assert
            Assert.False(result);
        }



        // [Fact] — пометка что это тест
        // public void ЧтоТестируем_Условие_ЧтоОжидаем()
        // {
        // AAA - структура
        //     // Arrange — готовим данные
        //     var hours = 18;
        //
        //     // Act — вызываем метод
        //     var result = WorkSchedule.IsWithinWorkingHours(hours);
        //
        //     // Assert — проверяем результат
        //     Assert.False(result);   // ожидаем false
        //     Assert.True(x)          // ожидаем true
        //     Assert.Equal(5, result) // ожидаем конкретное значение
        // }

    }
}
