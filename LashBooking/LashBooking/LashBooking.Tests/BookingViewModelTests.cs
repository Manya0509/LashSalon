using LashBooking.Web.MVC.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LashBooking.Tests
{
    public class BookingViewModelTests
    {
        [Fact]
        public void ClientName_TooShort_IsInvalid() // Имя из 1 символа → ошибка
        {
            var model = new BookingViewModel
            {
                ClientName = "А",  
                ClientPhone = "+79001234567",
                ServiceId = 1,
                SelectedTime = "2026-06-05T10:00:00"
            };

            var results = new List<ValidationResult>();
            var isValid = Validator.TryValidateObject(model,
                new ValidationContext(model), results, validateAllProperties: true);

            Assert.False(isValid);
        }

        [Fact]
        public void ClientName_TooLong_IsInvalid() // Имя из 101 символа → ошибка
        {
            var model = new BookingViewModel
            {
                ClientName = new string('А', 101),
                ClientPhone = "+79001234567",
                ServiceId = 1,
                SelectedTime = "2026-06-05T10:00:00"
            };

            var results = new List<ValidationResult>();
            var isValid = Validator.TryValidateObject(model,
                new ValidationContext(model), results, validateAllProperties: true);

            Assert.False(isValid);
        }

        [Fact]
        public void ClientPhone_InvalidFormat_IsInvalid() // Неверный телефон → ошибка
        {
            var model = new BookingViewModel
            {
                ClientName = "Ирина",
                ClientPhone = "qwe",
                ServiceId = 1,
                SelectedTime = "2026-06-05T10:00:00"
            };

            var results = new List<ValidationResult>();
            var isValid = Validator.TryValidateObject(model,
                new ValidationContext(model), results, validateAllProperties: true);

            Assert.False(isValid);
        }

        [Fact]
        public void ValidModel_IsValid() // Все поля корректны → ок
        {
            var model = new BookingViewModel
            {
                ClientName = "Ирина",
                ClientPhone = "+79001234567",
                ServiceId = 1,
                SelectedTime = "2026-06-05T10:00:00"
            };

            var results = new List<ValidationResult>();
            var isValid = Validator.TryValidateObject(model,
                new ValidationContext(model), results, validateAllProperties: true);

            Assert.True(isValid);
        }

        [Fact]
        public void ServiceId_Zero_IsInvalid() // ServiceId = 0 → ошибка
        {
            var model = new BookingViewModel
            {
                ClientName = "Ирина",
                ClientPhone = "+79001234567",
                ServiceId = 0,
                SelectedTime = "2026-06-05T10:00:00"
            };

            var results = new List<ValidationResult>();
            var isValid = Validator.TryValidateObject(model,
                new ValidationContext(model), results, validateAllProperties: true);

            Assert.False(isValid);
        }

        [Fact]
        public void SelectedTime_Empty_IsInvalid() // Пустое время → ошибка
        {
            var model = new BookingViewModel
            {
                ClientName = "Ирина",
                ClientPhone = "+79001234567",
                ServiceId = 0,
                SelectedTime = ""
            };

            var results = new List<ValidationResult>();
            var isValid = Validator.TryValidateObject(model,
                new ValidationContext(model), results, validateAllProperties: true);

            Assert.False(isValid);
        }
    }
}
