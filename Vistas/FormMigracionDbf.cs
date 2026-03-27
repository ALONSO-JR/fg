using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using SistemaGestionCRA.Datos;
using SistemaGestionCRA.Logica;

namespace SistemaGestionCRA.Vistas
{
    public partial class FormMigracionDbf : Form
    {
        private readonly BaseDatosHelper _db;
        private readonly MigracionDbfLogica _logica;
        private string? _rutaDbf;
        private List<string> _camposDbf = new();
        private Dictionary<string, ComboBox> _mapeos = new();
        private Button btnImportar;

        public FormMigracionDbf(BaseDatosHelper db)
        {
            _db = db;
            _logica = new MigracionDbfLogica(db);
            InitializeComponentManual();
        }

        private void InitializeComponentManual()
        {
            this.Text = "Migración desde ABIES (DBF)";
            this.Size = new Size(500, 600);
            this.StartPosition = FormStartPosition.CenterParent;

            var lblInstruccion = new Label { Text = "1. Seleccione el archivo .DBF de Socios:", Location = new Point(20, 20), AutoSize = true };
            var btnSeleccionar = new Button { Text = "Examinar...", Location = new Point(20, 45), Size = new Size(100, 30) };
            btnSeleccionar.Click += (s, e) => SeleccionarArchivo();

            var grpMapeo = new GroupBox { Text = "2. Mapeo de Campos", Location = new Point(20, 100), Size = new Size(440, 350) };

            string[] camposDestino = { "RUT", "Nombre", "Apellidos", "Curso", "Rol" };
            int currentY = 30;
            foreach (var campo in camposDestino) {
                grpMapeo.Controls.Add(new Label { Text = campo + ":", Location = new Point(20, currentY), Size = new Size(100, 20) });
                var cmb = new ComboBox { Location = new Point(130, currentY), Size = new Size(200, 25), DropDownStyle = ComboBoxStyle.DropDownList };
                _mapeos.Add(campo, cmb);
                grpMapeo.Controls.Add(cmb);
                currentY += 40;
            }

            btnImportar = new Button { Text = "INICIAR IMPORTACIÓN", Location = new Point(150, 480), Size = new Size(200, 40), BackColor = Color.FromArgb(40, 167, 69), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Enabled = false };
            btnImportar.Click += (s, e) => EjecutarImportacion();

            this.Controls.AddRange(new Control[] { lblInstruccion, btnSeleccionar, grpMapeo, btnImportar });
        }

        private void SeleccionarArchivo()
        {
            using var ofd = new OpenFileDialog { Filter = "Archivos DBF (*.dbf)|*.dbf" };
            if (ofd.ShowDialog() == DialogResult.OK) {
                _rutaDbf = ofd.FileName;
                _camposDbf = _logica.ObtenerCamposDbf(_rutaDbf);
                foreach (var cmb in _mapeos.Values) {
                    cmb.Items.Clear();
                    cmb.Items.AddRange(_camposDbf.ToArray());
                }
                btnImportar.Enabled = true;
                MessageBox.Show($"Archivo cargado: {Path.GetFileName(_rutaDbf)}");
            }
        }

        private void EjecutarImportacion()
        {
            if (string.IsNullOrEmpty(_rutaDbf)) return;
            var mapeoFinal = _mapeos.ToDictionary(k => k.Key, v => v.Value.SelectedItem?.ToString() ?? "");

            var (procesados, errores, log) = _logica.ImportarSocios(_rutaDbf, mapeoFinal);

            MessageBox.Show($"Importación finalizada.\nProcesados: {procesados}\nErrores: {errores}", "Resultado", MessageBoxButtons.OK, MessageBoxIcon.Information);
            if (errores > 0) {
                File.WriteAllText("log_migracion_dbf.txt", log);
                MessageBox.Show("Se ha generado un archivo log_migracion_dbf.txt con los detalles.");
            }
            this.DialogResult = DialogResult.OK;
        }
    }
}
