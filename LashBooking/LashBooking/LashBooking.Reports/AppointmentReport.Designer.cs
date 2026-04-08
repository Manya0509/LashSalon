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

            // Новые элементы
            var lblPeriod = new XRLabel();
            var headerPhone = new XRTableCell();
            var headerDuration = new XRTableCell();
            var cellPhone = new XRTableCell();
            var cellDuration = new XRTableCell();

            ((ISupportInitialize)(this.headerTable)).BeginInit();
            ((ISupportInitialize)(this.dataTable)).BeginInit();
            ((ISupportInitialize)(this.objectDataSource1)).BeginInit();
            ((ISupportInitialize)(this)).BeginInit();

            // --- Отступы ---
            this.topMarginBand1.HeightF = 20F;
            this.topMarginBand1.Name = "topMarginBand1";
            this.bottomMarginBand1.HeightF = 20F;
            this.bottomMarginBand1.Name = "bottomMarginBand1";

            // --- Пустая основная полоса ---
            this.detailBand1.HeightF = 0F;
            this.detailBand1.Name = "detailBand1";

            // ===== ШАПКА ОТЧЁТА (тёмный фон) =====
            var darkBg = System.Drawing.Color.FromArgb(45, 45, 45);
            var white = System.Drawing.Color.White;

            // Название студии
            this.lblTitle.ExpressionBindings.AddRange(new ExpressionBinding[] {
        new ExpressionBinding("BeforePrint", "Text", "[Name]")
    });
            this.lblTitle.LocationFloat = new DevExpress.Utils.PointFloat(0F, 10F);
            this.lblTitle.SizeF = new System.Drawing.SizeF(1040F, 35F);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Font = new DevExpress.Drawing.DXFont("Arial", 18F,
                DevExpress.Drawing.DXFontStyle.Bold);
            this.lblTitle.TextAlignment = DevExpress.XtraPrinting.TextAlignment.MiddleCenter;
            this.lblTitle.ForeColor = white;
            this.lblTitle.BackColor = darkBg;
            this.lblTitle.Padding = new DevExpress.XtraPrinting.PaddingInfo(10, 10, 5, 0, 100F);

            // Период
            lblPeriod.ExpressionBindings.AddRange(new ExpressionBinding[] {
        new ExpressionBinding("BeforePrint", "Text", "[Period]")
    });
            lblPeriod.LocationFloat = new DevExpress.Utils.PointFloat(0F, 45F);
            lblPeriod.SizeF = new System.Drawing.SizeF(1040F, 25F);
            lblPeriod.Name = "lblPeriod";
            lblPeriod.Font = new DevExpress.Drawing.DXFont("Arial", 11F);
            lblPeriod.TextAlignment = DevExpress.XtraPrinting.TextAlignment.MiddleCenter;
            lblPeriod.ForeColor = System.Drawing.Color.FromArgb(200, 200, 200);
            lblPeriod.BackColor = darkBg;
            lblPeriod.Padding = new DevExpress.XtraPrinting.PaddingInfo(10, 10, 0, 5, 100F);

            this.reportHeader.Controls.AddRange(new XRControl[] {
        this.lblTitle, lblPeriod, this.headerTable
    });
            this.reportHeader.HeightF = 100F;
            this.reportHeader.Name = "reportHeader";

            // ===== ШАПКА ТАБЛИЦЫ =====
            var headerFont = new DevExpress.Drawing.DXFont("Arial", 9F,
                DevExpress.Drawing.DXFontStyle.Bold);
            var headerBg = System.Drawing.Color.FromArgb(68, 68, 68);
            var headerFg = System.Drawing.Color.White;
            var borderColor = System.Drawing.Color.FromArgb(200, 200, 200);

            XRTableCell[] headerCells = new XRTableCell[] {
        this.headerDate, this.headerTime, headerPhone,
        this.headerClient, this.headerService, headerDuration,
        this.headerPrice, this.headerStatus
    };
            string[] headerTexts = new string[] {
        "Дата", "Время", "Телефон", "Клиент", "Услуга", "Длит.", "Цена", "Статус"
    };
            double[] weights = new double[] {
    0.9, 0.6, 1.1, 1.2, 1.5, 0.6, 0.7, 0.9
};

            string[] headerNames = new string[] {
        "headerDate", "headerTime", "headerPhone", "headerClient",
        "headerService", "headerDuration", "headerPrice", "headerStatus"
    };

            for (int i = 0; i < headerCells.Length; i++)
            {
                headerCells[i].Text = headerTexts[i];
                headerCells[i].Name = headerNames[i];
                headerCells[i].Font = headerFont;
                headerCells[i].BackColor = headerBg;
                headerCells[i].ForeColor = headerFg;
                headerCells[i].Weight = weights[i];
                headerCells[i].Padding = new DevExpress.XtraPrinting.PaddingInfo(5, 5, 3, 3, 100F);
                headerCells[i].Borders = DevExpress.XtraPrinting.BorderSide.All;
                headerCells[i].BorderColor = borderColor;
                headerCells[i].TextAlignment = DevExpress.XtraPrinting.TextAlignment.MiddleCenter;
            }

            this.headerRow.Cells.AddRange(headerCells);
            this.headerRow.Name = "headerRow";
            this.headerRow.Weight = 1D;

            this.headerTable.LocationFloat = new DevExpress.Utils.PointFloat(0F, 75F);
            this.headerTable.Rows.AddRange(new XRTableRow[] { this.headerRow });
            this.headerTable.SizeF = new System.Drawing.SizeF(1040F, 25F);
            this.headerTable.Name = "headerTable";

            // ===== СТРОКИ ДАННЫХ =====
            var dataFont = new DevExpress.Drawing.DXFont("Arial", 8.5F);
            var stripeBg = System.Drawing.Color.FromArgb(245, 245, 245);

            XRTableCell[] dataCells = new XRTableCell[] {
        this.cellDate, this.cellTime, cellPhone,
        this.cellClient, this.cellService, cellDuration,
        this.cellPrice, this.cellStatus
    };
            string[] dataFields = new string[] {
        "[Date]", "[Time]", "[ClientPhone]",
        "[ClientName]", "[ServiceName]", "[Duration]",
        "[Price]", "[Status]"
    };
            string[] dataNames = new string[] {
        "cellDate", "cellTime", "cellPhone", "cellClient",
        "cellService", "cellDuration", "cellPrice", "cellStatus"
    };

            for (int i = 0; i < dataCells.Length; i++)
            {
                dataCells[i].ExpressionBindings.AddRange(new ExpressionBinding[] {
            new ExpressionBinding("BeforePrint", "Text", dataFields[i])
        });
                dataCells[i].Name = dataNames[i];
                dataCells[i].Font = dataFont;
                dataCells[i].Weight = weights[i];
                dataCells[i].Padding = new DevExpress.XtraPrinting.PaddingInfo(5, 5, 3, 3, 100F);
                dataCells[i].Borders = DevExpress.XtraPrinting.BorderSide.All;
                dataCells[i].BorderColor = borderColor;
            }

            this.dataRow.Cells.AddRange(dataCells);
            this.dataRow.Name = "dataRow";
            this.dataRow.Weight = 1D;

            this.dataTable.LocationFloat = new DevExpress.Utils.PointFloat(0F, 0F);
            this.dataTable.Rows.AddRange(new XRTableRow[] { this.dataRow });
            this.dataTable.SizeF = new System.Drawing.SizeF(1040F, 25F);
            this.dataTable.Name = "dataTable";

            // Чередование строк
            var styleEven = new DevExpress.XtraReports.UI.XRControlStyle();
            styleEven.Name = "EvenRow";
            styleEven.BackColor = stripeBg;
            this.StyleSheet.Add(styleEven);
            this.detailBand2 = new DetailBand();
            this.detailBand2.Controls.AddRange(new XRControl[] { this.dataTable });
            this.detailBand2.HeightF = 25F;
            this.detailBand2.Name = "detailBand2";
            this.detailBand2.EvenStyleName = "EvenRow";

            // DetailReport — привязан к коллекции Rows
            this.detailReport.Bands.AddRange(new Band[] { this.detailBand2 });
            this.detailReport.DataMember = "Rows";
            this.detailReport.DataSource = this.objectDataSource1;
            this.detailReport.Level = 0;
            this.detailReport.Name = "detailReport";

            // Источник данных
            this.objectDataSource1.DataSource =
                typeof(LashBooking.Reports.Models.AppointmentReportModel);
            this.objectDataSource1.Name = "objectDataSource1";

            // ===== ПОДВАЛ =====
            this.reportFooter = new ReportFooterBand();
            this.lblTotalText = new XRLabel();
            this.lblTotalSum = new XRLabel();
            var lblCount = new XRLabel();
            var lblGenerated = new XRLabel();
            var footerLine = new XRLabel();

            // Разделительная линия
            footerLine.LocationFloat = new DevExpress.Utils.PointFloat(0F, 0F);
            footerLine.SizeF = new System.Drawing.SizeF(1040F, 2F);
            footerLine.Name = "footerLine";
            footerLine.BackColor = System.Drawing.Color.FromArgb(68, 68, 68);
            footerLine.Text = "";

            // ИТОГО
            this.lblTotalText.Text = "ИТОГО (завершённые):";
            this.lblTotalText.LocationFloat = new DevExpress.Utils.PointFloat(0F, 10F);
            this.lblTotalText.SizeF = new System.Drawing.SizeF(500F, 25F);
            this.lblTotalText.Name = "lblTotalText";
            this.lblTotalText.Font = new DevExpress.Drawing.DXFont("Arial", 11F,
                DevExpress.Drawing.DXFontStyle.Bold);
            this.lblTotalText.TextAlignment =
                DevExpress.XtraPrinting.TextAlignment.MiddleRight;
            this.lblTotalText.Padding = new DevExpress.XtraPrinting.PaddingInfo(0, 10, 0, 0, 100F);

            this.lblTotalSum.ExpressionBindings.AddRange(new ExpressionBinding[] {
        new ExpressionBinding("BeforePrint", "Text", "[TotalSum]")
    });
            this.lblTotalSum.LocationFloat = new DevExpress.Utils.PointFloat(500F, 10F);
            this.lblTotalSum.SizeF = new System.Drawing.SizeF(540F, 25F);
            this.lblTotalSum.Name = "lblTotalSum";
            this.lblTotalSum.Font = new DevExpress.Drawing.DXFont("Arial", 11F,
                DevExpress.Drawing.DXFontStyle.Bold);
            this.lblTotalSum.TextAlignment =
                DevExpress.XtraPrinting.TextAlignment.MiddleLeft;

            // Количество записей
            lblCount.ExpressionBindings.AddRange(new ExpressionBinding[] {
        new ExpressionBinding("BeforePrint", "Text", "[TotalCount]")
    });
            lblCount.LocationFloat = new DevExpress.Utils.PointFloat(0F, 40F);
            lblCount.SizeF = new System.Drawing.SizeF(520F, 20F);
            lblCount.Name = "lblCount";
            lblCount.Font = new DevExpress.Drawing.DXFont("Arial", 9F);
            lblCount.ForeColor = System.Drawing.Color.FromArgb(100, 100, 100);
            lblCount.TextAlignment = DevExpress.XtraPrinting.TextAlignment.MiddleLeft;
            lblCount.Padding = new DevExpress.XtraPrinting.PaddingInfo(5, 0, 0, 0, 100F);

            // Дата генерации
            lblGenerated.ExpressionBindings.AddRange(new ExpressionBinding[] {
        new ExpressionBinding("BeforePrint", "Text", "[GeneratedDate]")
    });
            lblGenerated.LocationFloat = new DevExpress.Utils.PointFloat(540F, 40F);
            lblGenerated.SizeF = new System.Drawing.SizeF(500F, 20F);
            lblGenerated.Name = "lblGenerated";
            lblGenerated.Font = new DevExpress.Drawing.DXFont("Arial", 9F);
            lblGenerated.ForeColor = System.Drawing.Color.FromArgb(100, 100, 100);
            lblGenerated.TextAlignment = DevExpress.XtraPrinting.TextAlignment.MiddleRight;
            lblGenerated.Padding = new DevExpress.XtraPrinting.PaddingInfo(0, 5, 0, 0, 100F);

            this.reportFooter.Controls.AddRange(new XRControl[] {
        footerLine, this.lblTotalText, this.lblTotalSum, lblCount, lblGenerated
    });
            this.reportFooter.HeightF = 65F;
            this.reportFooter.Name = "reportFooter";

            // ===== СБОРКА ОТЧЁТА =====
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
            this.Margins = new System.Drawing.Printing.Margins(20, 20, 20, 20);
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
