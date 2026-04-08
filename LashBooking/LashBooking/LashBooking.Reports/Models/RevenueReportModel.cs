using System.Collections.Generic;

namespace LashBooking.Reports.Models
{
    public class RevenueReportModel
    {
        public string Name { get; set; }
        public string Period { get; set; }
        public string TotalRevenue { get; set; }
        public string TotalAvgCheck { get; set; }
        public string GeneratedDate { get; set; }
        public List<RevenueReportRow> Rows { get; set; }
    }
}
