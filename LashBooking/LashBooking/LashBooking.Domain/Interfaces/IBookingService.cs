using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LashBooking.Domain.Models;

namespace LashBooking.Domain.Interfaces
{
    // Контракт сервиса бронирования.
    public interface IBookingService
    {
        // Создать новую запись.
        Task<ServiceResult> CreateBookingAsync(
            int serviceId,
            string selectedTime,
            string clientName,
            string clientPhone,
            int? clientId);
    }
}

