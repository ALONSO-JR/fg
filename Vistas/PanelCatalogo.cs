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
    public partial class PanelCatalogo : UserControl
    {
        private readonly BaseDatosHelper _db;
        private DataGridView dgvEjemplares;
        private TextBox txtBuscar;
        private Button btnNuevo, btnEditar, btnEtiquetas;

        public PanelCatalogo(BaseDatosHelper db)
        {
            _db = db;
            InitializeComponentManual();
            CargarEjemplares();
        }

        private void InitializeComponentManual()
        {
            this.Dock = DockStyle.Fill;
            this.BackColor = Color.White;

            var lblTitulo = new Label { Text = "Catálogo de Ejemplares", Font = new Font("Segoe UI", 16, FontStyle.Bold), Location = new Point(20, 20), AutoSize = true };

            txtBuscar = new TextBox { Location = new Point(20, 70), Size = new Size(300, 25), PlaceholderText = "Buscar por título, autor o código..." };
            txtBuscar.TextChanged += (s, e) => CargarEjemplares(txtBuscar.Text);

            btnNuevo = CrearBotonAccion("Nuevo Ejemplar", 340, 70, Color.FromArgb(40, 167, 69));
            btnNuevo.Click += (s, e) => AbrirFormEjemplar(null);

            btnEditar = CrearBotonAccion("Editar", 460, 70, Color.FromArgb(0, 120, 215));
            btnEditar.Click += (s, e) => {
                if (dgvEjemplares.SelectedRows.Count > 0) {
                    var ejemplar = (Ejemplar)dgvEjemplares.SelectedRows[0].DataBoundItem;
                    AbrirFormEjemplar(ejemplar);
                }
            };

            btnEtiquetas = CrearBotonAccion("Imprimir Etiqueta", 580, 70, Color.FromArgb(108, 117, 125));

            var btnMarc = CrearBotonAccion("Exportar MARC21", 720, 70, Color.FromArgb(0, 123, 255));
            btnMarc.Click += (s, e) => {
                using var sfd = new SaveFileDialog { Filter = "XML Files (*.xml)|*.xml", FileName = "Catalogo_MARC21.xml" };
                if (sfd.ShowDialog() == DialogResult.OK) {
                    using var conexion = _db.ObtenerConexion();
                    conexion.Open();
                    var ejemplares = conexion.Query<Ejemplar>("SELECT * FROM Ejemplares").ToList();
                    var marc = new Logica.Marc21Servicio();
                    string res = marc.ExportarAMarcXml(sfd.FileName, ejemplares);
                    if (res == "OK") MessageBox.Show("Catálogo exportado en formato MARC21 XML.");
                    else MessageBox.Show("Error: " + res);
                }
            };
            this.Controls.Add(btnMarc);

            btnEtiquetas.Click += (s, e) => {
                if (dgvEjemplares.SelectedRows.Count > 0) {
                    var ejemplar = (Ejemplar)dgvEjemplares.SelectedRows[0].DataBoundItem;
                    using var sfd = new SaveFileDialog { Filter = "PDF Files (*.pdf)|*.pdf", FileName = $"Etiqueta_{ejemplar.CodigoBarras}.pdf" };
                    if (sfd.ShowDialog() == DialogResult.OK) {
                        var imp = new Logica.ImpresionServicio();
                        string res = imp.GenerarEtiquetasPdf(sfd.FileName, ejemplar);
                        if (res == "OK") MessageBox.Show("Etiqueta generada.");
                        else MessageBox.Show("Error: " + res);
                    }
                }
            };

            dgvEjemplares = new DataGridView {
                Location = new Point(20, 110),
                Size = new Size(this.Width - 60, this.Height - 150),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                AllowUserToAddRows = false,
                ReadOnly = true,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };

            this.Controls.AddRange(new Control[] { lblTitulo, txtBuscar, btnNuevo, btnEditar, btnEtiquetas, dgvEjemplares });
        }

        private Button CrearBotonAccion(string texto, int x, int y, Color color) {
            return new Button { Text = texto, Location = new Point(x, y), Size = new Size(130, 30), BackColor = color, ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
        }

        private void CargarEjemplares(string filtro = "") {
            using var conexion = _db.ObtenerConexion();
            conexion.Open();
            string sql = "SELECT * FROM Ejemplares WHERE Titulo LIKE @Filtro OR Autor LIKE @Filtro OR CodigoBarras LIKE @Filtro OR ISBN LIKE @Filtro";
            var lista = conexion.Query<Ejemplar>(sql, new { Filtro = $"%{filtro}%" }).ToList();
            dgvEjemplares.DataSource = lista;
        }

        private void AbrirFormEjemplar(Ejemplar? ejemplar) {
            using var form = new FormEjemplarDetalle(_db, ejemplar);
            if (form.ShowDialog() == DialogResult.OK) CargarEjemplares();
        }
    }

    public partial class FormEjemplarDetalle : Form
    {
        private readonly BaseDatosHelper _db;
        private Ejemplar? _ejemplar;
        private TextBox txtCodigoBarras, txtISBN, txtTitulo, txtAutor, txtEditorial, txtAnio, txtDewey, txtCutter, txtUbicacion;
        private ComboBox cmbTipo, cmbEstado;
        private Button btnGuardar;

        public FormEjemplarDetalle(BaseDatosHelper db, Ejemplar? ejemplar = null)
        {
            _db = db;
            _ejemplar = ejemplar;
            InitializeComponentManual();
            if (_ejemplar != null) CargarDatos();
        }

        private void InitializeComponentManual()
        {
            this.Text = _ejemplar == null ? "Nuevo Ejemplar" : "Editar Ejemplar";
            this.Size = new Size(500, 550);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;

            int labelX = 20, inputX = 150, currentY = 20, gap = 35;

            this.Controls.Add(new Label { Text = "Código de Barras:", Location = new Point(labelX, currentY), Size = new Size(120, 20) });
            txtCodigoBarras = new TextBox { Location = new Point(inputX, currentY), Size = new Size(300, 25) };
            currentY += gap;

            this.Controls.Add(new Label { Text = "Título:", Location = new Point(labelX, currentY) });
            txtTitulo = new TextBox { Location = new Point(inputX, currentY), Size = new Size(300, 25) };
            currentY += gap;

            this.Controls.Add(new Label { Text = "Autor:", Location = new Point(labelX, currentY) });
            txtAutor = new TextBox { Location = new Point(inputX, currentY), Size = new Size(300, 25) };
            currentY += gap;

            this.Controls.Add(new Label { Text = "ISBN:", Location = new Point(labelX, currentY) });
            txtISBN = new TextBox { Location = new Point(inputX, currentY), Size = new Size(300, 25) };
            currentY += gap;

            this.Controls.Add(new Label { Text = "Editorial:", Location = new Point(labelX, currentY) });
            txtEditorial = new TextBox { Location = new Point(inputX, currentY), Size = new Size(300, 25) };
            currentY += gap;

            this.Controls.Add(new Label { Text = "Año / Tipo:", Location = new Point(labelX, currentY) });
            txtAnio = new TextBox { Location = new Point(inputX, currentY), Size = new Size(80, 25) };
            cmbTipo = new ComboBox { Location = new Point(inputX + 90, currentY), Size = new Size(210, 25), DropDownStyle = ComboBoxStyle.DropDownList };
            cmbTipo.Items.AddRange(new string[] { "Libro", "Revista", "Manual", "Audiovisual", "Recurso Pedagógico" });
            currentY += gap;

            this.Controls.Add(new Label { Text = "Clasif. Dewey:", Location = new Point(labelX, currentY) });
            txtDewey = new TextBox { Location = new Point(inputX, currentY), Size = new Size(100, 25) };
            this.Controls.Add(new Label { Text = "Cutter:", Location = new Point(inputX + 110, currentY), Size = new Size(50, 20) });
            txtCutter = new TextBox { Location = new Point(inputX + 170, currentY), Size = new Size(130, 25) };
            currentY += gap;

            this.Controls.Add(new Label { Text = "Ubicación:", Location = new Point(labelX, currentY) });
            txtUbicacion = new TextBox { Location = new Point(inputX, currentY), Size = new Size(300, 25) };
            currentY += gap;

            this.Controls.Add(new Label { Text = "Estado:", Location = new Point(labelX, currentY) });
            cmbEstado = new ComboBox { Location = new Point(inputX, currentY), Size = new Size(300, 25), DropDownStyle = ComboBoxStyle.DropDownList };
            cmbEstado.Items.AddRange(new string[] { "Disponible", "Prestado", "Perdido", "Dañado", "En Reparación" });
            cmbEstado.SelectedItem = "Disponible";
            currentY += gap;

            btnGuardar = new Button { Text = "Guardar Ejemplar", Location = new Point(175, 460), Size = new Size(150, 40), BackColor = Color.FromArgb(40, 167, 69), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnGuardar.Click += BtnGuardar_Click;
            this.Controls.Add(btnGuardar);
        }

        private void CargarDatos()
        {
            if (_ejemplar == null) return;
            txtCodigoBarras.Text = _ejemplar.CodigoBarras;
            txtTitulo.Text = _ejemplar.Titulo;
            txtAutor.Text = _ejemplar.Autor;
            txtISBN.Text = _ejemplar.ISBN;
            txtEditorial.Text = _ejemplar.Editorial;
            txtAnio.Text = _ejemplar.Anio.ToString();
            cmbTipo.SelectedItem = _ejemplar.Tipo;
            txtDewey.Text = _ejemplar.ClasificacionDewey;
            txtCutter.Text = _ejemplar.Cutter;
            txtUbicacion.Text = _ejemplar.Ubicacion;
            cmbEstado.SelectedItem = _ejemplar.Estado;
        }

        private async void BtnGuardar_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtCodigoBarras.Text) || string.IsNullOrWhiteSpace(txtTitulo.Text))
            {
                MessageBox.Show("Código de Barras y Título son obligatorios.");
                return;
            }

            using var conexion = _db.ObtenerConexion();
            conexion.Open();

            int anio = 0;
            int.TryParse(txtAnio.Text, out anio);

            string sql;
            var param = new {
                CodigoBarras = txtCodigoBarras.Text,
                Titulo = txtTitulo.Text,
                Autor = txtAutor.Text,
                ISBN = txtISBN.Text,
                Editorial = txtEditorial.Text,
                Anio = anio,
                Tipo = cmbTipo.SelectedItem?.ToString(),
                ClasificacionDewey = txtDewey.Text,
                Cutter = txtCutter.Text,
                Ubicacion = txtUbicacion.Text,
                Estado = cmbEstado.SelectedItem?.ToString(),
                Id = _ejemplar?.Id ?? 0
            };

            if (_ejemplar == null)
            {
                sql = @"INSERT INTO Ejemplares (CodigoBarras, Titulo, Autor, ISBN, Editorial, Anio, Tipo, ClasificacionDewey, Cutter, Ubicacion, Estado)
                        VALUES (@CodigoBarras, @Titulo, @Autor, @ISBN, @Editorial, @Anio, @Tipo, @ClasificacionDewey, @Cutter, @Ubicacion, @Estado)";
            }
            else
            {
                sql = @"UPDATE Ejemplares SET CodigoBarras=@CodigoBarras, Titulo=@Titulo, Autor=@Autor, ISBN=@ISBN, Editorial=@Editorial, Anio=@Anio,
                        Tipo=@Tipo, ClasificacionDewey=@ClasificacionDewey, Cutter=@Cutter, Ubicacion=@Ubicacion, Estado=@Estado WHERE Id=@Id";
            }

            try {
                await conexion.ExecuteAsync(sql, param);
                this.DialogResult = DialogResult.OK;
                this.Close();
            } catch (Exception ex) {
                MessageBox.Show("Error al guardar: " + ex.Message);
            }
        }
    }
}
