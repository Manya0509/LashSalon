using System;                            
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LashBooking.Domain.Entities            
{
    public class AboutInfo                      
    {
        public int Id { get; set; }               // Первичный ключ
        public string MasterName { get; set; } = string.Empty;        // Имя мастера
        public string Role { get; set; } = string.Empty;        // Роль/специализация
        public string Experience { get; set; } = string.Empty;        // Бейдж опыта
        public string Quote { get; set; } = string.Empty;        // Цитата в блоке
        public string AboutText { get; set; } = string.Empty;        // Текст блока "Обо мне" 
        public string EducationText { get; set; } = string.Empty;       // Текст блока "Образование"
        public string Address { get; set; } = string.Empty;        // Адрес
        public string WorkingHours { get; set; } = string.Empty;        // Часы работы
        public string Phone { get; set; } = string.Empty;        // Телефон
        public string? WhatsAppLink { get; set; }        // Ссылка на WhatsApp
        public string? TelegramLink { get; set; }        // Ссылка на Telegram
        public string? PhotoFileName { get; set; }        // Имя файла фото мастера 
    }
}
