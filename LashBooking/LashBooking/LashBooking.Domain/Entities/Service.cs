namespace LashBooking.Domain.Entities
{
    public class Service
    {
        public int Id { get; set; } // Id услуги
        public string Name { get; set; } // Название услуги
        public string? Description { get; set; } // Описание услуги
        public decimal Price { get; set; } // Цена
        public int DurationMinutes { get; set; } // Продолжительность(в минутах)
        public bool IsActive { get; set; } = true; // Активна ли услуга

        public virtual ICollection<Appointment> Appointments { get; set; } = new List<Appointment>(); //у одной услуги может быть много записей
    }
}