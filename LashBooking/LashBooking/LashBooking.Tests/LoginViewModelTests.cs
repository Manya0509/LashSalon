using LashBooking.Web.MVC.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LashBooking.Tests
{
    public class LoginViewModelTests
    {
        [Fact]
        public void LoginPhone_InvalidFormat_IsInvalid() // Телефон "qwe" → ошибка
        {
            var model = new LoginViewModel
            { 
                LoginPassword = "password",
                LoginPhone = "qwe",
            };

            var results = new List<ValidationResult>();
            var isValid = Validator.TryValidateObject(model,
                new ValidationContext(model), results, validateAllProperties: true);

            Assert.False(isValid);
        }

        [Fact]
        public void LoginPhone_Empty_IsInvalid() // Пустой телефон → ошибка
        {
            var model = new LoginViewModel
            {
                LoginPassword = "password",
                LoginPhone = "",
            };

            var results = new List<ValidationResult>();
            var isValid = Validator.TryValidateObject(model,
                new ValidationContext(model), results, validateAllProperties: true);

            Assert.False(isValid);
        }

        [Fact]
        public void LoginPassword_Empty_IsInvalid() // Пустой пароль → ошибка
        {
            var model = new LoginViewModel
            {
                LoginPassword = "",
                LoginPhone = "qwe",
            };

            var results = new List<ValidationResult>();
            var isValid = Validator.TryValidateObject(model,
                new ValidationContext(model), results, validateAllProperties: true);

            Assert.False(isValid);
        }

        [Fact]
        public void ValidModel_IsValid() // Все поля корректны → ок
        {
            var model = new LoginViewModel
            {
                LoginPassword = "password",
                LoginPhone = "+79001234567",
            };

            var results = new List<ValidationResult>();
            var isValid = Validator.TryValidateObject(model,
                new ValidationContext(model), results, validateAllProperties: true);

            Assert.True(isValid);
        }
    }
}
