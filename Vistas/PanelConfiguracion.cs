using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using SistemaGestionCRA.Datos;
using SistemaGestionCRA.Logica;

namespace SistemaGestionCRA.Vistas
{
    public partial class PanelConfiguracion : UserControl
    {
        private readonly BaseDatosHelper _db;
        private readonly string _dbPath;
        private RespaldoLogica _respaldo;

        public PanelConfiguracion(BaseDatosHelper db)
        {
            _db = db;

            // Obtener ruta de BD real (simplificado para este paso)
            string appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            _dbPath = Path.Combine(appData, "SistemaCRA", "cra_database.db");
            _respaldo = new RespaldoLogica(_dbPath);

            InitializeComponentManual();
        }

        private void InitializeComponentManual()
        {
            this.Dock = DockStyle.Fill;
            this.BackColor = Color.White;

            var lblTitulo = new Label { Text = "Configuración y Respaldos", Font = new Font("Segoe UI", 16, FontStyle.Bold), Location = new Point(20, 20), AutoSize = true };

            var grpRespaldo = new GroupBox { Text = "Mantenimiento de Base de Datos", Location = new Point(20, 70), Size = new Size(400, 200) };

            var btnRespaldo = new Button { Text = "Crear Copia de Seguridad", Location = new Point(50, 40), Size = new Size(300, 40), BackColor = Color.FromArgb(0, 120, 215), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnRespaldo.Click += (s, e) => {
                using var fbd = new FolderBrowserDialog();
                if (fbd.ShowDialog() == DialogResult.OK) {
                    string res = _respaldo.RealizarRespaldo(fbd.SelectedPath);
                    if (res.StartsWith("OK")) MessageBox.Show("Copia creada con éxito.");
                    else MessageBox.Show(res);
                }
            };

            var btnRestaurar = new Button { Text = "Restaurar Base de Datos", Location = new Point(50, 100), Size = new Size(300, 40), BackColor = Color.FromArgb(108, 117, 125), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnRestaurar.Click += (s, e) => {
                using var ofd = new OpenFileDialog { Filter = "SQLite Database (*.db)|*.db" };
                if (ofd.ShowDialog() == DialogResult.OK) {
                    if (MessageBox.Show("Esto sobreescribirá todos los datos actuales. ¿Continuar?", "Confirmar Restauración", MessageBoxButtons.YesNo) == DialogResult.Yes) {
                        string res = _respaldo.RestaurarRespaldo(ofd.FileName);
                        if (res == "OK") MessageBox.Show("Restauración completada. Reinicie la aplicación.");
                        else MessageBox.Show(res);
                    }
                }
            };

            grpRespaldo.Controls.AddRange(new Control[] { btnRespaldo, btnRestaurar });

            var grpMigracion = new GroupBox { Text = "Migración de Datos", Location = new Point(20, 300), Size = new Size(400, 120) };
            var btnImportarABIES = new Button { Text = "Importar desde ABIES (CSV)", Location = new Point(50, 40), Size = new Size(300, 40), BackColor = Color.FromArgb(40, 167, 69), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };

            grpMigracion.Controls.Add(btnImportarABIES);

            this.Controls.AddRange(new Control[] { lblTitulo, grpRespaldo, grpMigracion });
        }
    }
}
