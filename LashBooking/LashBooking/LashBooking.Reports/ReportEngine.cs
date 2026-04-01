using DevExpress.XtraPrinting;
using LashBooking.Reports.Models;
using System.Collections.Generic;
using System.IO;

namespace LashBooking.Reports
{
    // Движок отчётов. Аналог XtraReportEngine из примера репетитора.
    // Принимает модель с данными, возвращает массив байтов (файл).
    // Контроллер потом отдаёт эти байты браузеру как скачиваемый файл.
    public class ReportEngine
    {
        // Генерирует отчёт по записям.
        //
        // model — данные для отчёта (заголовок + строки таблицы)
        // format — формат файла: "pdf", "docx" или "rtf"
        //
        // Возвращает byte[] — готовый файл в памяти
        public byte[] GenerateAppointmentReport(
            AppointmentReportModel model, string format = "pdf")
        {
            var stream = new MemoryStream();

            // Создаём отчёт и передаём данные
            // using — когда блок закончится, отчёт освободит память
            using (var report = new AppointmentReport())
            {
                // DataSource ожидает список (как в примере репетитора)
                var data = new List<AppointmentReportModel>();
                data.Add(model);
                report.DataSource = data;

                // Экспортируем в нужный формат
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
    }
}
