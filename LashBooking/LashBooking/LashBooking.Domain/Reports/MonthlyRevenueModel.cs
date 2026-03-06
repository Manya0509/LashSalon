using System;
using System.Collections.Generic;

namespace LashBooking.Domain.Reports
{
    public class MonthlyRevenueModel
    {
        public string ReportTitle { get; set; } = "Финансовый отчет";
        public int Year { get; set; }
        public decimal TotalRevenue { get; set; }
        public int TotalAppointments { get; set; }
        public decimal AverageCheck { get; set; }

        // Детализация по месяцам
        public List<MonthlyDetailModel> MonthlyDetails { get; set; } = new();

        // Для динамического размера шрифта (как в методичке)
        public int TitleSize { get; set; } = 14;
    }
}