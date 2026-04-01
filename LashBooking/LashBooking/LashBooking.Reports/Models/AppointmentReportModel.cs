using System.Collections.Generic;

namespace LashBooking.Reports.Models
{
    public class AppointmentReportModel
    {
        public string Name { get; set; }
        public string TotalSum { get; set; }
        public List<AppointmentReportRow> Rows { get; set; }
    }
}
