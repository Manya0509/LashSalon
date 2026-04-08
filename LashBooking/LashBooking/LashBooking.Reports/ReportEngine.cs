using DevExpress.XtraPrinting;
using LashBooking.Reports.Models;
using System.Collections.Generic;
using System.IO;

namespace LashBooking.Reports
{
    public class ReportEngine
    {
        public byte[] GenerateAppointmentReport(
            AppointmentReportModel model, string format = "pdf")
        {
            var stream = new MemoryStream();

            using (var report = new AppointmentReport())
            {
                var data = new List<AppointmentReportModel>();
                data.Add(model);
                report.DataSource = data;

                switch (format.ToLower())
                {
                    case "pdf":
                        report.ExportToPdf(stream);
                        break;
                    case "docx":
                        var docxOptions = new DocxExportOptions()
                        {
                            ExportMode = DocxExportMode.SingleFile,
                            TableLayout = true,
                            KeepRowHeight = true
                        };
                        report.ExportToDocx(stream, docxOptions);
                        break;
                    case "rtf":
                        report.ExportToRtf(stream);
                        break;
                    default:
                        report.ExportToPdf(stream);
                        break;
                }
            }

            return stream.ToArray();
        }

        public byte[] GenerateRevenueReport(
            RevenueReportModel model, string format = "pdf")
        {
            var stream = new MemoryStream();

            using (var report = new RevenueReport())
            {
                var data = new List<RevenueReportModel>();
                data.Add(model);
                report.DataSource = data;

                switch (format.ToLower())
                {
                    case "pdf":
                        report.ExportToPdf(stream);
                        break;
                    case "docx":
                        var docxOptions = new DocxExportOptions()
                        {
                            ExportMode = DocxExportMode.SingleFile,
                            TableLayout = true,
                            KeepRowHeight = true
                        };
                        report.ExportToDocx(stream, docxOptions);
                        break;
                    default:
                        report.ExportToPdf(stream);
                        break;
                }
            }

            return stream.ToArray();
        }
    }
}
