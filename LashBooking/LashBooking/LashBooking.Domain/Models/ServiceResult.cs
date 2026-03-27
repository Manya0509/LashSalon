using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LashBooking.Domain.Models
{
    // "Конвертик" с результатом операции.
    // Сервис не знает про веб (TempData, Redirect и т.д.),
    // поэтому возвращает результат через этот класс.
    // Контроллер читает Success и Message и решает что показать пользователю.
    public class ServiceResult
    {
        // Успешна ли операция
        public bool Success { get; set; }

        // Сообщение для пользователя (или причина ошибки)
        public string Message { get; set; } = "";

        // Быстрый способ создать успешный результат
        // Пример: return ServiceResult.Ok("Запись создана!");
        public static ServiceResult Ok(string message = "") =>
            new() { Success = true, Message = message };

        // Быстрый способ создать ошибку
        // Пример: return ServiceResult.Fail("Этот день недоступен");
        public static ServiceResult Fail(string message) =>
            new() { Success = false, Message = message };
    }
}

