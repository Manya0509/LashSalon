using DevExpress.DataAccess.ObjectBinding;
using DevExpress.XtraReports.UI;
using System.ComponentModel;

namespace LashBooking.Reports
{
    partial class RevenueReport
    {
        private IContainer components = null;

        private TopMarginBand topMargin;
        private BottomMarginBand bottomMargin;
        private DetailBand detailBand1;
        private ReportHeaderBand reportHeader;
        private DetailReportBand detailReport;
        private DetailBand detailBand2;
        private ReportFooterBand reportFooter;

        private XRLabel lblTitle;
        private XRTable headerTable;
        private XRTableRow headerRow;
        private XRTable dataTable;
        private XRTableRow dataRow;
        private ObjectDataSource objectDataSource1;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.components = new Container();

            this.topMargin = new TopMarginBand();
            this.bottomMargin = new BottomMarginBand();
            this.detailBand1 = new DetailBand();
            this.reportHeader = new ReportHeaderBand();
            this.lblTitle = new XRLabel();
            this.detailReport = new DetailReportBand();
            this.detailBand2 = new DetailBand();
            this.reportFooter = new ReportFooterBand();

            this.headerTable = new XRTable();
            this.headerRow = new XRTableRow();
            this.dataTable = new XRTable();
            this.dataRow = new XRTableRow();

            this.objectDataSource1 = new ObjectDataSource(this.components);

            var lblPeriod = new XRLabel();

            ((ISupportInitialize)(this.headerTable)).BeginInit();
            ((ISupportInitialize)(this.dataTable)).BeginInit();
            ((ISupportInitialize)(this.objectDataSource1)).BeginInit();
            ((ISupportInitialize)(this)).BeginInit();

            // --- Отступы ---
            this.topMargin.HeightF = 20F;
            this.topMargin.Name = "topMargin";
            this.bottomMargin.HeightF = 20F;
            this.bottomMargin.Name = "bottomMargin";

            this.detailBand1.HeightF = 0F;
            this.detailBand1.Name = "detailBand1";

            // ===== ШАПКА =====
            var darkBg = System.Drawing.Color.FromArgb(45, 45, 45);
            var white = System.Drawing.Color.White;

            this.lblTitle.ExpressionBindings.AddRange(new ExpressionBinding[] {
                new ExpressionBinding("BeforePrint", "Text", "[Name]")
            });
            this.lblTitle.LocationFloat = new DevExpress.Utils.PointFloat(0F, 10F);
            this.lblTitle.SizeF = new System.Drawing.SizeF(760F, 35F);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Font = new DevExpress.Drawing.DXFont("Arial", 18F,
                DevExpress.Drawing.DXFontStyle.Bold);
            this.lblTitle.TextAlignment = DevExpress.XtraPrinting.TextAlignment.MiddleCenter;
            this.lblTitle.ForeColor = white;
            this.lblTitle.BackColor = darkBg;
            this.lblTitle.Padding = new DevExpress.XtraPrinting.PaddingInfo(10, 10, 5, 0, 100F);

            lblPeriod.ExpressionBindings.AddRange(new ExpressionBinding[] {
                new ExpressionBinding("BeforePrint", "Text", "[Period]")
            });
            lblPeriod.LocationFloat = new DevExpress.Utils.PointFloat(0F, 45F);
            lblPeriod.SizeF = new System.Drawing.SizeF(760F, 25F);
            lblPeriod.Name = "lblPeriod";
            lblPeriod.Font = new DevExpress.Drawing.DXFont("Arial", 11F);
            lblPeriod.TextAlignment = DevExpress.XtraPrinting.TextAlignment.MiddleCenter;
            lblPeriod.ForeColor = System.Drawing.Color.FromArgb(200, 200, 200);
            lblPeriod.BackColor = darkBg;
            lblPeriod.Padding = new DevExpress.XtraPrinting.PaddingInfo(10, 10, 0, 5, 100F);

            // ===== ШАПКА ТАБЛИЦЫ =====
            var headerFont = new DevExpress.Drawing.DXFont("Arial", 10F,
                DevExpress.Drawing.DXFontStyle.Bold);
            var headerBg = System.Drawing.Color.FromArgb(68, 68, 68);
            var borderColor = System.Drawing.Color.FromArgb(200, 200, 200);

            string[] headerTexts = { "Месяц", "Записей", "Завершённых", "Выручка", "Средний чек" };
            double[] weights = { 1.5, 0.8, 0.8, 1.0, 1.0 };
            string[] headerNames = { "hMonth", "hTotal", "hCompleted", "hRevenue", "hAvg" };

            for (int i = 0; i < headerTexts.Length; i++)
            {
                var cell = new XRTableCell();
                cell.Text = headerTexts[i];
                cell.Name = headerNames[i];
                cell.Font = headerFont;
                cell.BackColor = headerBg;
                cell.ForeColor = white;
                cell.Weight = weights[i];
                cell.Padding = new DevExpress.XtraPrinting.PaddingInfo(8, 8, 5, 5, 100F);
                cell.Borders = DevExpress.XtraPrinting.BorderSide.All;
                cell.BorderColor = borderColor;
                cell.TextAlignment = DevExpress.XtraPrinting.TextAlignment.MiddleCenter;
                this.headerRow.Cells.Add(cell);
            }
            this.headerRow.Name = "headerRow";
            this.headerRow.Weight = 1D;

            this.headerTable.LocationFloat = new DevExpress.Utils.PointFloat(0F, 75F);
            this.headerTable.Rows.AddRange(new XRTableRow[] { this.headerRow });
            this.headerTable.SizeF = new System.Drawing.SizeF(760F, 30F);
            this.headerTable.Name = "headerTable";

            this.reportHeader.Controls.AddRange(new XRControl[] {
                this.lblTitle, lblPeriod, this.headerTable
            });
            this.reportHeader.HeightF = 110F;
            this.reportHeader.Name = "reportHeader";

            // ===== СТРОКИ ДАННЫХ =====
            var dataFont = new DevExpress.Drawing.DXFont("Arial", 9.5F);
            var stripeBg = System.Drawing.Color.FromArgb(245, 245, 245);

            string[] dataFields = { "[Month]", "[TotalCount]", "[CompletedCount]", "[Revenue]", "[AvgCheck]" };
            string[] dataNames = { "cMonth", "cTotal", "cCompleted", "cRevenue", "cAvg" };

            for (int i = 0; i < dataFields.Length; i++)
            {
                var cell = new XRTableCell();
                cell.ExpressionBindings.AddRange(new ExpressionBinding[] {
                    new ExpressionBinding("BeforePrint", "Text", dataFields[i])
                });
                cell.Name = dataNames[i];
                cell.Font = dataFont;
                cell.Weight = weights[i];
                cell.Padding = new DevExpress.XtraPrinting.PaddingInfo(8, 8, 4, 4, 100F);
                cell.Borders = DevExpress.XtraPrinting.BorderSide.All;
                cell.BorderColor = borderColor;
                cell.TextAlignment = i == 0
                    ? DevExpress.XtraPrinting.TextAlignment.MiddleLeft
                    : DevExpress.XtraPrinting.TextAlignment.MiddleCenter;
                this.dataRow.Cells.Add(cell);
            }
            this.dataRow.Name = "dataRow";
            this.dataRow.Weight = 1D;

            this.dataTable.LocationFloat = new DevExpress.Utils.PointFloat(0F, 0F);
            this.dataTable.Rows.AddRange(new XRTableRow[] { this.dataRow });
            this.dataTable.SizeF = new System.Drawing.SizeF(760F, 28F);
            this.dataTable.Name = "dataTable";

            // Чередование строк
            var styleEven = new XRControlStyle();
            styleEven.Name = "EvenRow";
            styleEven.BackColor = stripeBg;
            this.StyleSheet.Add(styleEven);

            this.detailBand2.Controls.AddRange(new XRControl[] { this.dataTable });
            this.detailBand2.HeightF = 28F;
            this.detailBand2.Name = "detailBand2";
            this.detailBand2.EvenStyleName = "EvenRow";

            this.detailReport.Bands.AddRange(new Band[] { this.detailBand2 });
            this.detailReport.DataMember = "Rows";
            this.detailReport.DataSource = this.objectDataSource1;
            this.detailReport.Level = 0;
            this.detailReport.Name = "detailReport";

            // Источник данных
            this.objectDataSource1.DataSource =
                typeof(LashBooking.Reports.Models.RevenueReportModel);
            this.objectDataSource1.Name = "objectDataSource1";

            // ===== ПОДВАЛ =====
            var footerLine = new XRLabel();
            var lblTotalLabel = new XRLabel();
            var lblTotalValue = new XRLabel();
            var lblAvgLabel = new XRLabel();
            var lblAvgValue = new XRLabel();
            var lblGenerated = new XRLabel();

            footerLine.LocationFloat = new DevExpress.Utils.PointFloat(0F, 0F);
            footerLine.SizeF = new System.Drawing.SizeF(760F, 2F);
            footerLine.Name = "footerLine";
            footerLine.BackColor = System.Drawing.Color.FromArgb(68, 68, 68);
            footerLine.Text = "";

            // Итого выручка
            lblTotalLabel.Text = "Итого выручка:";
            lblTotalLabel.LocationFloat = new DevExpress.Utils.PointFloat(0F, 10F);
            lblTotalLabel.SizeF = new System.Drawing.SizeF(380F, 25F);
            lblTotalLabel.Name = "lblTotalLabel";
            lblTotalLabel.Font = new DevExpress.Drawing.DXFont("Arial", 11F,
                DevExpress.Drawing.DXFontStyle.Bold);
            lblTotalLabel.TextAlignment = DevExpress.XtraPrinting.TextAlignment.MiddleRight;
            lblTotalLabel.Padding = new DevExpress.XtraPrinting.PaddingInfo(0, 10, 0, 0, 100F);

            lblTotalValue.ExpressionBindings.AddRange(new ExpressionBinding[] {
                new ExpressionBinding("BeforePrint", "Text", "[TotalRevenue]")
            });
            lblTotalValue.LocationFloat = new DevExpress.Utils.PointFloat(380F, 10F);
            lblTotalValue.SizeF = new System.Drawing.SizeF(380F, 25F);
            lblTotalValue.Name = "lblTotalValue";
            lblTotalValue.Font = new DevExpress.Drawing.DXFont("Arial", 11F,
                DevExpress.Drawing.DXFontStyle.Bold);
            lblTotalValue.TextAlignment = DevExpress.XtraPrinting.TextAlignment.MiddleLeft;

            // Средний чек за период
            lblAvgLabel.Text = "Средний чек за период:";
            lblAvgLabel.LocationFloat = new DevExpress.Utils.PointFloat(0F, 35F);
            lblAvgLabel.SizeF = new System.Drawing.SizeF(380F, 25F);
            lblAvgLabel.Name = "lblAvgLabel";
            lblAvgLabel.Font = new DevExpress.Drawing.DXFont("Arial", 10F);
            lblAvgLabel.ForeColor = System.Drawing.Color.FromArgb(100, 100, 100);
            lblAvgLabel.TextAlignment = DevExpress.XtraPrinting.TextAlignment.MiddleRight;
            lblAvgLabel.Padding = new DevExpress.XtraPrinting.PaddingInfo(0, 10, 0, 0, 100F);

            lblAvgValue.ExpressionBindings.AddRange(new ExpressionBinding[] {
                new ExpressionBinding("BeforePrint", "Text", "[TotalAvgCheck]")
            });
            lblAvgValue.LocationFloat = new DevExpress.Utils.PointFloat(380F, 35F);
            lblAvgValue.SizeF = new System.Drawing.SizeF(380F, 25F);
            lblAvgValue.Name = "lblAvgValue";
            lblAvgValue.Font = new DevExpress.Drawing.DXFont("Arial", 10F);
            lblAvgValue.ForeColor = System.Drawing.Color.FromArgb(100, 100, 100);
            lblAvgValue.TextAlignment = DevExpress.XtraPrinting.TextAlignment.MiddleLeft;

            // Дата генерации
            lblGenerated.ExpressionBindings.AddRange(new ExpressionBinding[] {
                new ExpressionBinding("BeforePrint", "Text", "[GeneratedDate]")
            });
            lblGenerated.LocationFloat = new DevExpress.Utils.PointFloat(0F, 65F);
            lblGenerated.SizeF = new System.Drawing.SizeF(760F, 20F);
            lblGenerated.Name = "lblGenerated";
            lblGenerated.Font = new DevExpress.Drawing.DXFont("Arial", 8F);
            lblGenerated.ForeColor = System.Drawing.Color.FromArgb(150, 150, 150);
            lblGenerated.TextAlignment = DevExpress.XtraPrinting.TextAlignment.MiddleRight;
            lblGenerated.Padding = new DevExpress.XtraPrinting.PaddingInfo(0, 5, 0, 0, 100F);

            this.reportFooter.Controls.AddRange(new XRControl[] {
                footerLine, lblTotalLabel, lblTotalValue,
                lblAvgLabel, lblAvgValue, lblGenerated
            });
            this.reportFooter.HeightF = 90F;
            this.reportFooter.Name = "reportFooter";

            // ===== СБОРКА =====
            this.Bands.AddRange(new Band[] {
                this.topMargin, this.bottomMargin,
                this.detailBand1, this.reportHeader,
                this.detailReport, this.reportFooter
            });
            this.ComponentStorage.AddRange(new IComponent[] { this.objectDataSource1 });
            this.DataSource = this.objectDataSource1;
            this.Font = new DevExpress.Drawing.DXFont("Arial", 9.75F);
            this.Margins = new System.Drawing.Printing.Margins(20, 20, 20, 20);
            this.Version = "23.1";

            ((ISupportInitialize)(this.headerTable)).EndInit();
            ((ISupportInitialize)(this.dataTable)).EndInit();
            ((ISupportInitialize)(this.objectDataSource1)).EndInit();
            ((ISupportInitialize)(this)).EndInit();
        }
    }
}
