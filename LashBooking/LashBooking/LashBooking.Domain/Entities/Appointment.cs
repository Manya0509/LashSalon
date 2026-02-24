namespace LashBooking.Domain.Entities
{
    public class Appointment
    {
        public int Id { get; set; } // Id услуги
        public DateTime DateStart { get; set; } // Дата и время начала процедуры
        public DateTime DateEnd { get; set; } // Дата и время окончания
        public string? Notes { get; set; } // Дополнительные заметки к записи
        public AppointmentStatus Status { get; set; } = AppointmentStatus.Scheduled; // Статус записи
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow; // Когда создана запись
        public int ClientId { get; set; } // Внешний ключ на клиента
        public int ServiceId { get; set; } // Внешний ключ на услугу
        public virtual Client Client { get; set; } // Навигационное свойство: какая запись принадлежит какому клиенту
        public virtual Service Service { get; set; } // Навигационное свойство: какая запись на какую услугу
    }

    // Перечисление статусов записи
    public enum AppointmentStatus
    {
        Scheduled = 1,   // Запланирована
        Confirmed = 2,   // Подтверждена
        Completed = 3,   // Выполнена
        Cancelled = 4,   // Отменена
        NoShow = 5       // Клиент не явился
    }
}