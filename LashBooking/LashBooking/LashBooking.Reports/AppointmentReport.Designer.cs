using DevExpress.DataAccess.ObjectBinding;
using DevExpress.XtraReports.UI;
using System.ComponentModel;

namespace LashBooking.Reports
{
    partial class AppointmentReport
    {
        private IContainer components = null;

        // Полосы отчёта (секции страницы)
        private TopMarginBand topMarginBand1;       // Верхний отступ
        private BottomMarginBand bottomMarginBand1;  // Нижний отступ
        private DetailBand detailBand1;              // Основная полоса (пустая)
        private ReportHeaderBand reportHeader;       // Заголовок отчёта
        private DetailReportBand detailReport;       // Полоса для таблицы данных
        private DetailBand detailBand2;              // Строки таблицы

        // Заголовок
        private XRLabel lblTitle;

        // Таблица-шапка (заголовки колонок)
        private XRTable headerTable;
        private XRTableRow headerRow;
        private XRTableCell headerDate;
        private XRTableCell headerTime;
        private XRTableCell headerClient;
        private XRTableCell headerService;
        private XRTableCell headerPrice;
        private XRTableCell headerStatus;

        // Таблица-данные (строки с записями)
        private XRTable dataTable;
        private XRTableRow dataRow;
        private XRTableCell cellDate;
        private XRTableCell cellTime;
        private XRTableCell cellClient;
        private XRTableCell cellService;
        private XRTableCell cellPrice;
        private XRTableCell cellStatus;

        // Источник данных
        private ObjectDataSource objectDataSource1;
        private ReportFooterBand reportFooter;
        private XRLabel lblTotalText;
        private XRLabel lblTotalSum;


        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.components = new Container();

            // --- Создаём все элементы ---
            this.topMarginBand1 = new TopMarginBand();
            this.bottomMarginBand1 = new BottomMarginBand();
            this.detailBand1 = new DetailBand();
            this.reportHeader = new ReportHeaderBand();
            this.lblTitle = new XRLabel();
            this.detailReport = new DetailReportBand();
            this.detailBand2 = new DetailBand();

            this.headerTable = new XRTable();
            this.headerRow = new XRTableRow();
            this.headerDate = new XRTableCell();
            this.headerTime = new XRTableCell();
            this.headerClient = new XRTableCell();
            this.headerService = new XRTableCell();
            this.headerPrice = new XRTableCell();
            this.headerStatus = new XRTableCell();

            this.dataTable = new XRTable();
            this.dataRow = new XRTableRow();
            this.cellDate = new XRTableCell();
            this.cellTime = new XRTableCell();
            this.cellClient = new XRTableCell();
            this.cellService = new XRTableCell();
            this.cellPrice = new XRTableCell();
            this.cellStatus = new XRTableCell();

            this.objectDataSource1 = new ObjectDataSource(this.components);

            ((ISupportInitialize)(this.headerTable)).BeginInit();
            ((ISupportInitialize)(this.dataTable)).BeginInit();
            ((ISupportInitialize)(this.objectDataSource1)).BeginInit();
            ((ISupportInitialize)(this)).BeginInit();

            // --- Верхний отступ ---
            this.topMarginBand1.HeightF = 30F;
            this.topMarginBand1.Name = "topMarginBand1";

            // --- Нижний отступ ---
            this.bottomMarginBand1.HeightF = 30F;
            this.bottomMarginBand1.Name = "bottomMarginBand1";

            // --- Пустая основная полоса ---
            this.detailBand1.HeightF = 0F;
            this.detailBand1.Name = "detailBand1";

            // ===== ЗАГОЛОВОК ОТЧЁТА =====
            // Привязан к полю [Name] из AppointmentReportModel
            this.lblTitle.ExpressionBindings.AddRange(new ExpressionBinding[] {
                new ExpressionBinding("BeforePrint", "Text", "[Name]")
            });
            this.lblTitle.LocationFloat = new DevExpress.Utils.PointFloat(0F, 0F);
            this.lblTitle.SizeF = new System.Drawing.SizeF(650F, 35F);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Font = new DevExpress.Drawing.DXFont("Arial", 14F,
                DevExpress.Drawing.DXFontStyle.Bold);
            this.lblTitle.TextAlignment =
                DevExpress.XtraPrinting.TextAlignment.MiddleCenter;

            this.reportHeader.Controls.AddRange(new XRControl[] { this.lblTitle });
            this.reportHeader.HeightF = 45F;
            this.reportHeader.Name = "reportHeader";

            // ===== ШАПКА ТАБЛИЦЫ (заголовки колонок) =====
            // Жирный шрифт + серый фон
            var headerFont = new DevExpress.Drawing.DXFont("Arial", 9F,
                DevExpress.Drawing.DXFontStyle.Bold);
            var headerBg = System.Drawing.Color.FromArgb(230, 230, 230);


            this.headerDate.Text = "Дата";
            this.headerDate.Name = "headerDate";
            this.headerDate.Font = headerFont;
            this.headerDate.BackColor = headerBg;
            this.headerDate.Weight = 1D;
            this.headerDate.Padding = new DevExpress.XtraPrinting.PaddingInfo(3, 3, 0, 0, 100F);
            this.headerDate.Borders = DevExpress.XtraPrinting.BorderSide.All;

            this.headerTime.Text = "Время";
            this.headerTime.Name = "headerTime";
            this.headerTime.Font = headerFont;
            this.headerTime.BackColor = headerBg;
            this.headerTime.Weight = 0.7D;
            this.headerTime.Padding = new DevExpress.XtraPrinting.PaddingInfo(3, 3, 0, 0, 100F);
            this.headerTime.Borders = DevExpress.XtraPrinting.BorderSide.All;

            this.headerClient.Text = "Клиент";
            this.headerClient.Name = "headerClient";
            this.headerClient.Font = headerFont;
            this.headerClient.BackColor = headerBg;
            this.headerClient.Weight = 1.5D;
            this.headerClient.Padding = new DevExpress.XtraPrinting.PaddingInfo(3, 3, 0, 0, 100F);
            this.headerClient.Borders = DevExpress.XtraPrinting.BorderSide.All;

            this.headerService.Text = "Услуга";
            this.headerService.Name = "headerService";
            this.headerService.Font = headerFont;
            this.headerService.BackColor = headerBg;
            this.headerService.Weight = 1.5D;
            this.headerService.Padding = new DevExpress.XtraPrinting.PaddingInfo(3, 3, 0, 0, 100F);
            this.headerService.Borders = DevExpress.XtraPrinting.BorderSide.All;

            this.headerPrice.Text = "Цена";
            this.headerPrice.Name = "headerPrice";
            this.headerPrice.Font = headerFont;
            this.headerPrice.BackColor = headerBg;
            this.headerPrice.Weight = 0.8D;
            this.headerPrice.Padding = new DevExpress.XtraPrinting.PaddingInfo(3, 3, 0, 0, 100F);
            this.headerPrice.Borders = DevExpress.XtraPrinting.BorderSide.All;

            this.headerStatus.Text = "Статус";
            this.headerStatus.Name = "headerStatus";
            this.headerStatus.Font = headerFont;
            this.headerStatus.BackColor = headerBg;
            this.headerStatus.Weight = 1D;
            this.headerStatus.Padding = new DevExpress.XtraPrinting.PaddingInfo(3, 3, 0, 0, 100F);
            this.headerStatus.Borders = DevExpress.XtraPrinting.BorderSide.All;

            this.headerRow.Cells.AddRange(new XRTableCell[] {
                this.headerDate, this.headerTime, this.headerClient,
                this.headerService, this.headerPrice, this.headerStatus
            });
            this.headerRow.Name = "headerRow";
            this.headerRow.Weight = 1D;

            this.headerTable.LocationFloat = new DevExpress.Utils.PointFloat(0F, 0F);
            this.headerTable.Rows.AddRange(new XRTableRow[] { this.headerRow });
            this.headerTable.SizeF = new System.Drawing.SizeF(650F, 25F);
            this.headerTable.Name = "headerTable";

            // ===== СТРОКИ ДАННЫХ =====
            // Привязки к полям из AppointmentReportRow
            var dataFont = new DevExpress.Drawing.DXFont("Arial", 9F);

            this.cellDate.ExpressionBindings.AddRange(new ExpressionBinding[] {
                new ExpressionBinding("BeforePrint", "Text", "[Date]")
            });
            this.cellDate.Name = "cellDate";
            this.cellDate.Font = dataFont;
            this.cellDate.Weight = 1D;
            this.cellDate.Padding = new DevExpress.XtraPrinting.PaddingInfo(3, 3, 0, 0, 100F);
            this.cellDate.Borders = DevExpress.XtraPrinting.BorderSide.All;

            this.cellTime.ExpressionBindings.AddRange(new ExpressionBinding[] {
                new ExpressionBinding("BeforePrint", "Text", "[Time]")
            });
            this.cellTime.Name = "cellTime";
            this.cellTime.Font = dataFont;
            this.cellTime.Weight = 0.7D;
            this.cellTime.Padding = new DevExpress.XtraPrinting.PaddingInfo(3, 3, 0, 0, 100F);
            this.cellTime.Borders = DevExpress.XtraPrinting.BorderSide.All;

            this.cellClient.ExpressionBindings.AddRange(new ExpressionBinding[] {
                new ExpressionBinding("BeforePrint", "Text", "[ClientName]")
            });
            this.cellClient.Name = "cellClient";
            this.cellClient.Font = dataFont;
            this.cellClient.Weight = 1.5D;
            this.cellClient.Padding = new DevExpress.XtraPrinting.PaddingInfo(3, 3, 0, 0, 100F);
            this.cellClient.Borders = DevExpress.XtraPrinting.BorderSide.All;

            this.cellService.ExpressionBindings.AddRange(new ExpressionBinding[] {
                new ExpressionBinding("BeforePrint", "Text", "[ServiceName]")
            });
            this.cellService.Name = "cellService";
            this.cellService.Font = dataFont;
            this.cellService.Weight = 1.5D;
            this.cellService.Padding = new DevExpress.XtraPrinting.PaddingInfo(3, 3, 0, 0, 100F);
            this.cellService.Borders = DevExpress.XtraPrinting.BorderSide.All;

            this.cellPrice.ExpressionBindings.AddRange(new ExpressionBinding[] {
                new ExpressionBinding("BeforePrint", "Text", "[Price]")
            });
            this.cellPrice.Name = "cellPrice";
            this.cellPrice.Font = dataFont;
            this.cellPrice.Weight = 0.8D;
            this.cellPrice.Padding = new DevExpress.XtraPrinting.PaddingInfo(3, 3, 0, 0, 100F);
            this.cellPrice.Borders = DevExpress.XtraPrinting.BorderSide.All;

            this.cellStatus.ExpressionBindings.AddRange(new ExpressionBinding[] {
                new ExpressionBinding("BeforePrint", "Text", "[Status]")
            });
            this.cellStatus.Name = "cellStatus";
            this.cellStatus.Font = dataFont;
            this.cellStatus.Weight = 1D;
            this.cellStatus.Padding = new DevExpress.XtraPrinting.PaddingInfo(3, 3, 0, 0, 100F);
            this.cellStatus.Borders = DevExpress.XtraPrinting.BorderSide.All;

            this.dataRow.Cells.AddRange(new XRTableCell[] {
                this.cellDate, this.cellTime, this.cellClient,
                this.cellService, this.cellPrice, this.cellStatus
            });
            this.dataRow.Name = "dataRow";
            this.dataRow.Weight = 1D;

            this.dataTable.LocationFloat = new DevExpress.Utils.PointFloat(0F, 0F);
            this.dataTable.Rows.AddRange(new XRTableRow[] { this.dataRow });
            this.dataTable.SizeF = new System.Drawing.SizeF(650F, 25F);
            this.dataTable.Name = "dataTable";

            // ===== СБОРКА ОТЧЁТА =====

            // Полоса шапки таблицы — вставляем в ReportHeader под заголовком
            this.reportHeader.Controls.Add(this.headerTable);
            this.headerTable.LocationFloat = new DevExpress.Utils.PointFloat(0F, 40F);
            this.reportHeader.HeightF = 70F;

            // Полоса данных — строки таблицы
            this.detailBand2.Controls.AddRange(new XRControl[] { this.dataTable });
            this.detailBand2.HeightF = 25F;
            this.detailBand2.Name = "detailBand2";

            // DetailReport привязан к коллекции Rows из AppointmentReportModel
            this.detailReport.Bands.AddRange(new Band[] { this.detailBand2 });
            this.detailReport.DataMember = "Rows";
            this.detailReport.DataSource = this.objectDataSource1;
            this.detailReport.Level = 0;
            this.detailReport.Name = "detailReport";

            // Источник данных — тип AppointmentReportModel
            this.objectDataSource1.DataSource =
                typeof(LashBooking.Reports.Models.AppointmentReportModel);
            this.objectDataSource1.Name = "objectDataSource1";

            // ===== ИТОГО =====
            this.reportFooter = new ReportFooterBand();
            this.lblTotalText = new XRLabel();
            this.lblTotalSum = new XRLabel();

            this.lblTotalText.Text = "ИТОГО:";
            this.lblTotalText.LocationFloat = new DevExpress.Utils.PointFloat(0F, 5F);
            this.lblTotalText.SizeF = new System.Drawing.SizeF(400F, 25F);
            this.lblTotalText.Name = "lblTotalText";
            this.lblTotalText.Font = new DevExpress.Drawing.DXFont("Arial", 11F,
                DevExpress.Drawing.DXFontStyle.Bold);
            this.lblTotalText.TextAlignment =
                DevExpress.XtraPrinting.TextAlignment.MiddleRight;
            this.lblTotalText.Padding = new DevExpress.XtraPrinting.PaddingInfo(0, 10, 0, 0, 100F);

            this.lblTotalSum.ExpressionBindings.AddRange(new ExpressionBinding[] {
                new ExpressionBinding("BeforePrint", "Text", "[TotalSum]")
            });
            this.lblTotalSum.LocationFloat = new DevExpress.Utils.PointFloat(400F, 5F);
            this.lblTotalSum.SizeF = new System.Drawing.SizeF(250F, 25F);
            this.lblTotalSum.Name = "lblTotalSum";
            this.lblTotalSum.Font = new DevExpress.Drawing.DXFont("Arial", 11F,
                DevExpress.Drawing.DXFontStyle.Bold);
            this.lblTotalSum.TextAlignment =
                DevExpress.XtraPrinting.TextAlignment.MiddleLeft;

            this.reportFooter.Controls.AddRange(new XRControl[] {
                this.lblTotalText, this.lblTotalSum });
            this.reportFooter.HeightF = 35F;
            this.reportFooter.Name = "reportFooter";

            // Собираем все полосы в отчёт
            this.Bands.AddRange(new Band[] {
                this.topMarginBand1,
                this.bottomMarginBand1,
                this.detailBand1,
                this.reportHeader,
                this.detailReport,
                this.reportFooter
            });
            this.ComponentStorage.AddRange(new IComponent[] {
                this.objectDataSource1
            });
            this.DataSource = this.objectDataSource1;
            this.Font = new DevExpress.Drawing.DXFont("Arial", 9.75F);
            this.Landscape = true;
            this.Version = "23.1";

            ((ISupportInitialize)(this.headerTable)).EndInit();
            ((ISupportInitialize)(this.dataTable)).EndInit();
            ((ISupportInitialize)(this.objectDataSource1)).EndInit();
            ((ISupportInitialize)(this)).EndInit();
        }
    }
}
