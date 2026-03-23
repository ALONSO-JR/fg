using System;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using SistemaGestionCRA.Datos;
using SistemaGestionCRA.Logica;
using SistemaGestionCRA.Modelos;
using Dapper;

namespace SistemaGestionCRA.Vistas
{
    public partial class PanelCirculacion : UserControl
    {
        private readonly BaseDatosHelper _db;
        private readonly CirculacionLogica _logica;
        private TextBox txtCodigoSocio, txtCodigoEjemplar;
        private Label lblSocioInfo, lblEjemplarInfo;
        private DataGridView dgvPrestamosSocio;
        private Socio? socioActual;
        private Ejemplar? ejemplarActual;

        public PanelCirculacion(BaseDatosHelper db)
        {
            _db = db;
            _logica = new CirculacionLogica(db);
            InitializeComponentManual();
        }

        private void InitializeComponentManual()
        {
            this.Dock = DockStyle.Fill;
            this.BackColor = Color.White;

            var lblTitulo = new Label { Text = "Préstamos y Devoluciones", Font = new Font("Segoe UI", 16, FontStyle.Bold), Location = new Point(20, 20), AutoSize = true };

            // Sección Socio
            var grpSocio = new GroupBox { Text = "Identificación de Socio", Location = new Point(20, 60), Size = new Size(450, 150) };
            txtCodigoSocio = new TextBox { Location = new Point(20, 30), Size = new Size(200, 25), PlaceholderText = "Escanear RUT o buscar..." };
            txtCodigoSocio.KeyDown += async (s, e) => { if (e.KeyCode == Keys.Enter) await BuscarSocio(); };

            lblSocioInfo = new Label { Text = "Socio: No seleccionado", Location = new Point(20, 70), Size = new Size(400, 60), Font = new Font("Segoe UI", 10, FontStyle.Italic) };
            grpSocio.Controls.AddRange(new Control[] { txtCodigoSocio, lblSocioInfo });

            // Sección Ejemplar
            var grpEjemplar = new GroupBox { Text = "Acción sobre Ejemplar", Location = new Point(490, 60), Size = new Size(450, 150) };
            txtCodigoEjemplar = new TextBox { Location = new Point(20, 30), Size = new Size(200, 25), PlaceholderText = "Escanear Código de Barras..." };
            txtCodigoEjemplar.KeyDown += async (s, e) => { if (e.KeyCode == Keys.Enter) await ProcesarEjemplar(); };

            lblEjemplarInfo = new Label { Text = "Ejemplar: No seleccionado", Location = new Point(20, 70), Size = new Size(400, 60), Font = new Font("Segoe UI", 10, FontStyle.Italic) };
            grpEjemplar.Controls.AddRange(new Control[] { txtCodigoEjemplar, lblEjemplarInfo });

            // Botones Rápidos
            var btnPrestar = new Button { Text = "PRÉSTAMO", Location = new Point(20, 220), Size = new Size(150, 40), BackColor = Color.FromArgb(0, 120, 215), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 10, FontStyle.Bold) };
            btnPrestar.Click += async (s, e) => await RealizarPrestamo();

            var btnDevolver = new Button { Text = "DEVOLUCIÓN", Location = new Point(180, 220), Size = new Size(150, 40), BackColor = Color.FromArgb(40, 167, 69), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 10, FontStyle.Bold) };
            btnDevolver.Click += async (s, e) => await RealizarDevolucion();

            // Grilla de Préstamos Activos
            var lblGrilla = new Label { Text = "Préstamos Activos del Socio:", Location = new Point(20, 280), AutoSize = true };
            dgvPrestamosSocio = new DataGridView {
                Location = new Point(20, 310),
                Size = new Size(this.Width - 60, this.Height - 350),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom,
                BackgroundColor = Color.WhiteSmoke,
                ReadOnly = true,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect
            };

            this.Controls.AddRange(new Control[] { lblTitulo, grpSocio, grpEjemplar, btnPrestar, btnDevolver, lblGrilla, dgvPrestamosSocio });
        }

        private async Task BuscarSocio()
        {
            string rut = ValidadorRut.FormatearRut(txtCodigoSocio.Text);
            using var conexion = _db.ObtenerConexion();
            conexion.Open();
            socioActual = await conexion.QueryFirstOrDefaultAsync<Socio>("SELECT * FROM Socios WHERE RUT = @RUT OR Id = @Id", new { RUT = rut, Id = txtCodigoSocio.Text });

            if (socioActual != null) {
                lblSocioInfo.Text = $"Socio: {socioActual.Nombre} {socioActual.Apellidos}\nCurso: {socioActual.Curso} | Estado: {socioActual.Estado}";
                lblSocioInfo.ForeColor = socioActual.Estado == "Activo" ? Color.Green : Color.Red;
                CargarPrestamosSocio();
                txtCodigoEjemplar.Focus();
            } else {
                lblSocioInfo.Text = "Socio no encontrado.";
                lblSocioInfo.ForeColor = Color.Red;
                dgvPrestamosSocio.DataSource = null;
            }
        }

        private void CargarPrestamosSocio()
        {
            if (socioActual == null) return;
            using var conexion = _db.ObtenerConexion();
            conexion.Open();
            string sql = @"SELECT p.Id, e.Titulo, e.CodigoBarras, p.FechaPrestamo, p.FechaVencimiento, p.Estado
                           FROM Prestamos p JOIN Ejemplares e ON p.EjemplarId = e.Id
                           WHERE p.SocioId = @SocioId AND p.Estado = 'Pendiente'";
            var lista = conexion.Query(sql, new { SocioId = socioActual.Id }).ToList();
            dgvPrestamosSocio.DataSource = lista;
        }

        private async Task ProcesarEjemplar()
        {
            using var conexion = _db.ObtenerConexion();
            conexion.Open();
            ejemplarActual = await conexion.QueryFirstOrDefaultAsync<Ejemplar>("SELECT * FROM Ejemplares WHERE CodigoBarras = @Codigo", new { Codigo = txtCodigoEjemplar.Text });

            if (ejemplarActual != null) {
                lblEjemplarInfo.Text = $"Ejemplar: {ejemplarActual.Titulo}\nAutor: {ejemplarActual.Autor}\nEstado actual: {ejemplarActual.Estado}";
                lblEjemplarInfo.ForeColor = ejemplarActual.Estado == "Disponible" ? Color.Green : Color.OrangeRed;
            } else {
                lblEjemplarInfo.Text = "Ejemplar no encontrado.";
                lblEjemplarInfo.ForeColor = Color.Red;
            }
        }

        private async Task RealizarPrestamo()
        {
            if (socioActual == null || ejemplarActual == null) {
                MessageBox.Show("Debe seleccionar un socio y un ejemplar.");
                return;
            }
            string resultado = await _logica.Prestar(socioActual.Id, ejemplarActual.Id);
            if (resultado == "OK") {
                MessageBox.Show("Préstamo realizado con éxito.");
                txtCodigoEjemplar.Clear();
                lblEjemplarInfo.Text = "Ejemplar: No seleccionado";
                CargarPrestamosSocio();
            } else {
                MessageBox.Show(resultado, "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private async Task RealizarDevolucion()
        {
            if (ejemplarActual == null) {
                MessageBox.Show("Debe escanear un ejemplar para devolver.");
                return;
            }
            string resultado = await _logica.Devolver(ejemplarActual.Id);
            if (resultado == "OK") {
                MessageBox.Show("Devolución procesada correctamente.");
                txtCodigoEjemplar.Clear();
                lblEjemplarInfo.Text = "Ejemplar: No seleccionado";
                if (socioActual != null) CargarPrestamosSocio();
            } else {
                MessageBox.Show(resultado, "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
    }
}
