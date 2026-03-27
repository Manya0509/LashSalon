using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LashBooking.Domain.Models;

namespace LashBooking.Domain.Interfaces
{
    // Контракт сервиса бронирования.
    // Отвечает за создание записи со всеми проверками:
    // — время в будущем?
    // — услуга существует?
    // — день/час не заблокирован?
    // — нет дубликата записи?
    // — клиент есть в базе или нужно создать?
    public interface IBookingService
    {
        // Создать новую запись.
        //
        // Параметры:
        //   serviceId    — какая услуга выбрана
        //   selectedTime — выбранное время (строка, например "2026-03-28T10:00:00")
        //   clientName   — имя клиента (из формы)
        //   clientPhone  — телефон клиента (из формы)
        //   clientId     — Id клиента из сессии (null если не авторизован)
        //
        // Возвращает ServiceResult:
        //   Success = true,  Message = "Вы успешно записаны! ..."
        //   Success = false, Message = "Этот день недоступен для записи."
        Task<ServiceResult> CreateBookingAsync(
            int serviceId,
            string selectedTime,
            string clientName,
            string clientPhone,
            int? clientId);
    }
}

