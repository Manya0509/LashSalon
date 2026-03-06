namespace LashBooking.Domain.Reports
{
    public class MonthlyDetailModel
    {
        public string MonthName { get; set; } = "";
        public int MonthNumber { get; set; }
        public decimal Revenue { get; set; }
        public int AppointmentsCount { get; set; }
        public string TopService { get; set; } = "";
    }
}