using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using SistemaGestionCRA.Datos;
using SistemaGestionCRA.Modelos;
using Dapper;

namespace SistemaGestionCRA.Vistas
{
    public partial class PanelInventario : UserControl
    {
        private readonly BaseDatosHelper _db;
        private DataGridView dgvInventario;
        private ComboBox cmbSeccion;
        private Button btnGenerarLista;
        private Button btnRegistrarHallazgo;

        public PanelInventario(BaseDatosHelper db)
        {
            _db = db;
            InitializeComponentManual();
        }

        private void InitializeComponentManual()
        {
            this.Dock = DockStyle.Fill;
            this.BackColor = Color.White;

            var lblTitulo = new Label { Text = "Control de Inventario", Font = new Font("Segoe UI", 16, FontStyle.Bold), Location = new Point(20, 20), AutoSize = true };

            var lblSeccion = new Label { Text = "Filtrar por Sección/Tipo:", Location = new Point(20, 70), AutoSize = true };
            cmbSeccion = new ComboBox { Location = new Point(180, 70), Size = new Size(200, 25), DropDownStyle = ComboBoxStyle.DropDownList };
            cmbSeccion.Items.AddRange(new string[] { "Todos", "Libro", "Revista", "Manual", "Audiovisual", "Recurso Pedagógico" });
            cmbSeccion.SelectedIndex = 0;

            btnGenerarLista = new Button { Text = "Generar Listado", Location = new Point(400, 70), Size = new Size(130, 30), BackColor = Color.FromArgb(0, 120, 215), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnGenerarLista.Click += (s, e) => CargarInventario();

            btnRegistrarHallazgo = new Button { Text = "Registrar Hallazgo/Baja", Location = new Point(540, 70), Size = new Size(180, 30), BackColor = Color.FromArgb(108, 117, 125), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };

            dgvInventario = new DataGridView {
                Location = new Point(20, 110),
                Size = new Size(this.Width - 60, this.Height - 150),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                ReadOnly = true,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };

            this.Controls.AddRange(new Control[] { lblTitulo, lblSeccion, cmbSeccion, btnGenerarLista, btnRegistrarHallazgo, dgvInventario });
        }

        private void CargarInventario()
        {
            using var conexion = _db.ObtenerConexion();
            conexion.Open();
            string sql = "SELECT CodigoBarras, Titulo, Autor, Ubicacion, Estado FROM Ejemplares";
            if (cmbSeccion.SelectedItem?.ToString() != "Todos") {
                sql += " WHERE Tipo = @Tipo";
            }
            var lista = conexion.Query(sql, new { Tipo = cmbSeccion.SelectedItem?.ToString() }).ToList();
            dgvInventario.DataSource = lista;
        }
    }
}
