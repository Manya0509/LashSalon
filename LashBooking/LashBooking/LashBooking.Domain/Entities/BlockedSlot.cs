using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LashBooking.Domain.Entities
{
    public class BlockedSlot
    {
        public int Id { get; set; }

        // Дата блокировки
        public DateTime Date { get; set; }

        // Если null — заблокирован весь день
        // Если указано — заблокирован конкретный час
        public int? BlockedHour { get; set; }

        // Причина (необязательно)
        public string? Reason { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
