using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LashBooking.Domain.Entities
{
    public class Review
    {
        public int Id { get; set; }                                     // Уникальный идентификатор отзыва

        public int ClientId { get; set; }                               // ID клиента, оставившего отзыв
        public virtual Client Client { get; set; }                      // Навигационное свойство - клиент

        public int Rating { get; set; }                                 // Оценка от 1 до 5 звёзд
        public string Text { get; set; } = "";                          // Текст отзыва

        public bool IsApproved { get; set; } = false;                   // Прошёл ли модерацию
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;      // Дата и время создания отзыва
        public bool IsRejected { get; set; } = false;  // Отклонён ли модератором
    }
}
