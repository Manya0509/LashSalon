using System.ComponentModel.DataAnnotations;

namespace LashBooking.Web.MVC.Models
{
    // описывает какие поля есть в форме и какие у них правила валидации
    public class LoginViewModel
    // Модель для страницы входа /Auth/Login
    {
        [Required(ErrorMessage = "Введите номер телефона")]
        // Поле обязательно к заполнению
        // Если пустое — ModelState.IsValid вернёт false
        // ErrorMessage — текст ошибки который увидит пользователь

        [RegularExpression(@"^[\d\+\-\(\)\s]{7,20}$",
        ErrorMessage = "Введите корректный номер телефона")]
        public string LoginPhone { get; set; } = "";
        // Номер телефона для входа
        // = "" — значение по умолчанию, чтобы не было null

        [Required(ErrorMessage = "Введите пароль")]
        // Тоже обязательное поле
        public string LoginPassword { get; set; } = "";
        // Пароль для входа
    }

    public class RegisterViewModel
    // Модель для страницы регистрации /Auth/Register
    {
        [Required(ErrorMessage = "Введите имя")]
        // Имя обязательно

        [StringLength(100, MinimumLength = 2, ErrorMessage = "Имя должно быть от 2 до 100 символов")]
        // Длина строки: минимум 2, максимум 100 символов
        // Защита от "А" и от огромных строк в базе данных
        public string RegName { get; set; } = "";
        // Имя клиента


        [Required(ErrorMessage = "Введите номер телефона")]
        // Телефон обязателен

        [RegularExpression(@"^[\d\+\-\(\)\s]{7,20}$",
            ErrorMessage = "Введите корректный номер телефона")]
        // Регулярное выражение — проверяет формат телефона
        // ^ — начало строки
        // [\d\+\-\(\)\s] — разрешены: цифры, +, -, (, ), пробел
        // {7,20} — от 7 до 20 символов
        // $ — конец строки
        // Пропустит: +7 (999) 410-38-01, 79994103801, +79994103801
        public string RegPhone { get; set; } = "";


        [Required(ErrorMessage = "Введите пароль")]
        [MinLength(6, ErrorMessage = "Пароль должен быть не менее 6 символов")]
        // Минимальная длина пароля — 6 символов
        // Защита от слишком простых паролей типа "123"
        public string RegPassword { get; set; } = "";


        [Required(ErrorMessage = "Повторите пароль")]
        [Compare("RegPassword", ErrorMessage = "Пароли не совпадают")]
        // Compare — сравнивает с другим полем модели
        // "RegPassword" — название поля с которым сравниваем (строка!)
        // Если RegPassword != RegConfirmPassword → ошибка "Пароли не совпадают"
        public string RegConfirmPassword { get; set; } = "";


        [EmailAddress(ErrorMessage = "Введите корректный email")]
        // Проверяет формат email: должна быть @ и домен
        // Пропустит: user@mail.ru, НЕ пропустит: просто "user"
        public string? RegEmail { get; set; }
        // ? — nullable, поле необязательное (нет [Required])
        // Клиент может не указывать email
    }
}
