using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using SistemaGestionCRA.Datos;
using ScottPlot.WinForms;
using Dapper;

namespace SistemaGestionCRA.Vistas
{
    public partial class PanelReportes : UserControl
    {
        private readonly BaseDatosHelper _db;
        private FormsPlot plotEstadisticas;
        private Button btnExportarPDF, btnExportarExcel;
        private Label lblStatsInfo;
        private DateTimePicker dtpDesde, dtpHasta;
        private Logica.ReporteMineducServicio _servicio;

        public PanelReportes(BaseDatosHelper db)
        {
            _db = db;
            _servicio = new Logica.ReporteMineducServicio(db);
            InitializeComponentManual();
            CargarGraficoPrestamos();
        }

        private void InitializeComponentManual()
        {
            this.Dock = DockStyle.Fill;
            this.BackColor = Color.White;

            var lblTitulo = new Label { Text = "Reportes y Estadísticas", Font = new Font("Segoe UI", 16, FontStyle.Bold), Location = new Point(20, 20), AutoSize = true };

            lblStatsInfo = new Label { Text = "Préstamos por Categoría de Socio", Font = new Font("Segoe UI", 12), Location = new Point(20, 70), AutoSize = true };

            plotEstadisticas = new FormsPlot {
                Location = new Point(20, 100),
                Size = new Size(this.Width - 300, 400),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom
            };

            var grpExportar = new GroupBox { Text = "Exportar Reportes MINEDUC", Location = new Point(this.Width - 260, 100), Size = new Size(240, 320), Anchor = AnchorStyles.Top | AnchorStyles.Right };

            grpExportar.Controls.Add(new Label { Text = "Desde:", Location = new Point(20, 30) });
            dtpDesde = new DateTimePicker { Location = new Point(20, 50), Size = new Size(200, 25), Format = DateTimePickerFormat.Short, Value = DateTime.Now.AddMonths(-1) };

            grpExportar.Controls.Add(new Label { Text = "Hasta:", Location = new Point(20, 85) });
            dtpHasta = new DateTimePicker { Location = new Point(20, 105), Size = new Size(200, 25), Format = DateTimePickerFormat.Short, Value = DateTime.Now };

            btnExportarPDF = new Button { Text = "Generar Circulación PDF", Location = new Point(20, 150), Size = new Size(200, 40), BackColor = Color.FromArgb(220, 53, 69), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnExportarPDF.Click += (s, e) => {
                using var sfd = new SaveFileDialog { Filter = "PDF Files (*.pdf)|*.pdf", FileName = "Reporte_Circulacion_CRA.pdf" };
                if (sfd.ShowDialog() == DialogResult.OK) {
                    string res = _servicio.GenerarReporteCirculacionPdf(sfd.FileName, dtpDesde.Value, dtpHasta.Value);
                    if (res == "OK") MessageBox.Show("Reporte generado con éxito.");
                    else MessageBox.Show("Error: " + res);
                }
            };

            btnExportarExcel = new Button { Text = "Exportar Inventario Excel", Location = new Point(20, 210), Size = new Size(200, 40), BackColor = Color.FromArgb(40, 167, 69), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnExportarExcel.Click += (s, e) => {
                using var sfd = new SaveFileDialog { Filter = "Excel Files (*.xlsx)|*.xlsx", FileName = "Inventario_CRA.xlsx" };
                if (sfd.ShowDialog() == DialogResult.OK) {
                    string res = _servicio.GenerarReporteInventarioExcel(sfd.FileName);
                    if (res == "OK") MessageBox.Show("Inventario exportado.");
                    else MessageBox.Show("Error: " + res);
                }
            };

            grpExportar.Controls.AddRange(new Control[] { dtpDesde, dtpHasta, btnExportarPDF, btnExportarExcel });

            this.Controls.AddRange(new Control[] { lblTitulo, lblStatsInfo, plotEstadisticas, grpExportar });
        }

        private void CargarGraficoPrestamos()
        {
            using var conexion = _db.ObtenerConexion();
            conexion.Open();

            string sql = @"SELECT s.Rol, COUNT(p.Id) as Cantidad
                           FROM Prestamos p JOIN Socios s ON p.SocioId = s.Id
                           GROUP BY s.Rol";
            var datos = conexion.Query(sql).ToList();

            if (datos.Count == 0) return;

            double[] values = datos.Select(d => (double)d.Cantidad).ToArray();
            string[] labels = datos.Select(d => (string)d.Rol).ToArray();

            plotEstadisticas.Plot.Clear();

            var slices = new List<ScottPlot.PieSlice>();
            for (int i = 0; i < values.Length; i++)
            {
                slices.Add(new ScottPlot.PieSlice { Value = values[i], Label = labels[i] });
            }

            var pie = plotEstadisticas.Plot.Add.Pie(slices);
            pie.ExplodeFraction = .1;
            plotEstadisticas.Plot.ShowLegend();

            plotEstadisticas.Refresh();
        }
    }
}
