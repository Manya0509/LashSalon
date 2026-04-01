namespace LashBooking.Reports.Models
{
    // Каждая строка — одна запись клиента.
    public class AppointmentReportRow
    {
        public string Date { get; set; }        // Дата: "27.03.2026"
        public string Time { get; set; }        // Время: "10:00"
        public string ClientName { get; set; }  // Имя клиента
        public string ClientPhone { get; set; } // Телефон
        public string ServiceName { get; set; } // Название услуги
        public string Duration { get; set; }    // Длительность: "60 мин"
        public string Price { get; set; }       // Цена: "2500 ₽"
        public string Status { get; set; }      // Статус: "Выполнена"
    }
}
