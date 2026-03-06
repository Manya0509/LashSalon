using System.Collections.Generic;

namespace LashBooking.Domain.Entities
{
    public class Client
    {
        public int Id { get; set; } // Id
        public string Name { get; set; } // Имя
        public string Phone { get; set; } // Телефон
        public string? Email { get; set; } // Почта (необязательное поле)
        public string? Notes { get; set; } // Заметки о клиенте
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow; // Дата создания записи
        public string? Password { get; set; }

        public virtual ICollection<Appointment> Appointments { get; set; } = new List<Appointment>(); // у одного клиента может быть много записей
        public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();
    }
}