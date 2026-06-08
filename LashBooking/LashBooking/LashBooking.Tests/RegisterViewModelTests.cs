using LashBooking.Web.MVC.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LashBooking.Tests
{
    public class RegisterViewModelTests
    {
        [Fact]
        public void RegName_TooShort_IsInvalid() // Имя из 1 символа → ошибка
        {
            var model = new RegisterViewModel
            {
                RegName = "q",
                RegPhone = "+79001234567",
                RegEmail = "test@mail.ru",
                RegPassword = "password",
                RegConfirmPassword = "password",
            };

            var results = new List<ValidationResult>();
            var isValid = Validator.TryValidateObject(model,
                new ValidationContext(model), results, validateAllProperties: true);

            Assert.False(isValid);
        }

        [Fact]
        public void RegName_TooLong_IsInvalid() // Имя из 101 символа → ошибка
        {
            var model = new RegisterViewModel
            {
                RegName = new string('А', 101),
                RegPhone = "+79001234567",
                RegEmail = "test@mail.ru",
                RegPassword = "password",
                RegConfirmPassword = "password",
            };

            var results = new List<ValidationResult>();
            var isValid = Validator.TryValidateObject(model,
                new ValidationContext(model), results, validateAllProperties: true);

            Assert.False(isValid);
        }

        [Fact]
        public void RegPhone_InvalidFormat_IsInvalid() // Телефон "qwe" → ошибка
        {
            var model = new RegisterViewModel
            {
                RegName = "Ирина",
                RegPhone = "qwe",
                RegEmail = "test@mail.ru",
                RegPassword = "password",
                RegConfirmPassword = "password",
            };

            var results = new List<ValidationResult>();
            var isValid = Validator.TryValidateObject(model,
                new ValidationContext(model), results, validateAllProperties: true);

            Assert.False(isValid);
        }

        [Fact]
        public void RegPassword_TooShort_IsInvalid() // Пароль менее 6 символов → ошибка
        {
            var model = new RegisterViewModel
            {
                RegName = "Ирина",
                RegPhone = "+79001234567",
                RegEmail = "test@mail.ru",
                RegPassword = "pass",
                RegConfirmPassword = "pass",
            };

            var results = new List<ValidationResult>();
            var isValid = Validator.TryValidateObject(model,
                new ValidationContext(model), results, validateAllProperties: true);

            Assert.False(isValid);
        }

        [Fact]
        public void RegConfirmPassword_Mismatch_IsInvalid() // Пароли не совпадают → ошибка
        {
            var model = new RegisterViewModel
            {
                RegName = "Ирина",
                RegPhone = "+79001234567",
                RegEmail = "test@mail.ru",
                RegPassword = "password",
                RegConfirmPassword = "pass",
            };

            var results = new List<ValidationResult>();
            var isValid = Validator.TryValidateObject(model,
                new ValidationContext(model), results, validateAllProperties: true);

            Assert.False(isValid);
        }

        [Fact]
        public void RegEmail_InvalidFormat_IsInvalid() // Неправильно написанный емаил → ошибка
        {
            var model = new RegisterViewModel
            {
                RegName = "Ирина",
                RegPhone = "+79001234567",
                RegEmail = "notanemail",
                RegPassword = "password",
                RegConfirmPassword = "password",
            };

            var results = new List<ValidationResult>();
            var isValid = Validator.TryValidateObject(model,
                new ValidationContext(model), results, validateAllProperties: true);

            Assert.False(isValid);
        }

        [Fact]
        public void RegEmail_Empty_IsValid() // Email не указан → ок (поле необязательное)
        {
            var model = new RegisterViewModel
            {
                RegName = "Ирина",
                RegPhone = "+79001234567",
                RegEmail = null,
                RegPassword = "password",
                RegConfirmPassword = "password",
            };

            var results = new List<ValidationResult>();
            var isValid = Validator.TryValidateObject(model,
                new ValidationContext(model), results, validateAllProperties: true);

            Assert.True(isValid);
        }

        [Fact]
        public void ValidModel_IsValid() // Все поля корректны → ок
        {
            var model = new RegisterViewModel
            {
                RegName = "Ирина",
                RegPhone = "+79001234567",
                RegEmail = "test@mail.ru",
                RegPassword = "password",
                RegConfirmPassword = "password",
            };

            var results = new List<ValidationResult>();
            var isValid = Validator.TryValidateObject(model,
                new ValidationContext(model), results, validateAllProperties: true);

            Assert.True(isValid);
        }
    }
}
