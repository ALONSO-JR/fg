using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using SistemaGestionCRA.Datos;
using SistemaGestionCRA.Modelos;
using SistemaGestionCRA.Logica;
using Dapper;

namespace SistemaGestionCRA.Vistas
{
    public partial class PanelSocios : UserControl
    {
        private readonly BaseDatosHelper _db;
        private DataGridView dgvSocios;
        private TextBox txtBuscar;
        private Button btnNuevo;
        private Button btnEditar;
        private Button btnImportar;

        public PanelSocios(BaseDatosHelper db)
        {
            _db = db;
            InitializeComponentManual();
            CargarSocios();
        }

        private void InitializeComponentManual()
        {
            this.Dock = DockStyle.Fill;
            this.BackColor = Color.White;

            var lblTitulo = new Label { Text = "Gestión de Socios", Font = new Font("Segoe UI", 16, FontStyle.Bold), Location = new Point(20, 20), AutoSize = true };

            txtBuscar = new TextBox { Location = new Point(20, 70), Size = new Size(300, 25), PlaceholderText = "Buscar por nombre o RUT..." };
            txtBuscar.TextChanged += (s, e) => CargarSocios(txtBuscar.Text);

            btnNuevo = CrearBotonAccion("Nuevo Socio", 340, 70, Color.FromArgb(40, 167, 69));
            btnNuevo.Click += (s, e) => AbrirFormSocio(null);

            btnEditar = CrearBotonAccion("Editar", 460, 70, Color.FromArgb(0, 120, 215));
            btnEditar.Click += (s, e) => {
                if (dgvSocios.SelectedRows.Count > 0)
                {
                    var socio = (Socio)dgvSocios.SelectedRows[0].DataBoundItem;
                    AbrirFormSocio(socio);
                }
            };

            btnImportar = CrearBotonAccion("Importar Excel", 580, 70, Color.FromArgb(108, 117, 125));

            dgvSocios = new DataGridView
            {
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

            this.Controls.AddRange(new Control[] { lblTitulo, txtBuscar, btnNuevo, btnEditar, btnImportar, dgvSocios });
        }

        private Button CrearBotonAccion(string texto, int x, int y, Color color)
        {
            return new Button { Text = texto, Location = new Point(x, y), Size = new Size(110, 30), BackColor = color, ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
        }

        private void CargarSocios(string filtro = "")
        {
            using var conexion = _db.ObtenerConexion();
            conexion.Open();
            string sql = "SELECT * FROM Socios WHERE Nombre LIKE @Filtro OR RUT LIKE @Filtro OR Apellidos LIKE @Filtro";
            var lista = conexion.Query<Socio>(sql, new { Filtro = $"%{filtro}%" }).ToList();
            dgvSocios.DataSource = lista;
        }

        private void AbrirFormSocio(Socio? socio)
        {
            using var form = new FormSocioDetalle(_db, socio);
            if (form.ShowDialog() == DialogResult.OK) CargarSocios();
        }
    }

    public partial class FormSocioDetalle : Form
    {
        private readonly BaseDatosHelper _db;
        private Socio? _socio;
        private TextBox txtRut, txtNombre, txtApellidos, txtEmail, txtTelefono;
        private ComboBox cmbNivel, cmbCurso, cmbRol;
        private Button btnGuardar;

        public FormSocioDetalle(BaseDatosHelper db, Socio? socio = null)
        {
            _db = db;
            _socio = socio;
            InitializeComponentManual();
            if (_socio != null) CargarDatos();
        }

        private void InitializeComponentManual()
        {
            this.Text = _socio == null ? "Nuevo Socio" : "Editar Socio";
            this.Size = new Size(400, 500);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;

            int labelX = 20, inputX = 150, currentY = 20, gap = 40;

            this.Controls.Add(new Label { Text = "RUT:", Location = new Point(labelX, currentY) });
            txtRut = new TextBox { Location = new Point(inputX, currentY), Size = new Size(200, 25) };
            currentY += gap;

            this.Controls.Add(new Label { Text = "Nombre:", Location = new Point(labelX, currentY) });
            txtNombre = new TextBox { Location = new Point(inputX, currentY), Size = new Size(200, 25) };
            currentY += gap;

            this.Controls.Add(new Label { Text = "Apellidos:", Location = new Point(labelX, currentY) });
            txtApellidos = new TextBox { Location = new Point(inputX, currentY), Size = new Size(200, 25) };
            currentY += gap;

            this.Controls.Add(new Label { Text = "Nivel:", Location = new Point(labelX, currentY) });
            cmbNivel = new ComboBox { Location = new Point(inputX, currentY), Size = new Size(200, 25), DropDownStyle = ComboBoxStyle.DropDownList };
            cmbNivel.Items.AddRange(new string[] { "Pre-básica", "Básica", "Media" });
            currentY += gap;

            this.Controls.Add(new Label { Text = "Curso:", Location = new Point(labelX, currentY) });
            cmbCurso = new ComboBox { Location = new Point(inputX, currentY), Size = new Size(200, 25) };
            cmbCurso.Items.AddRange(new string[] { "1° Básico", "2° Básico", "3° Básico", "4° Básico", "5° Básico", "6° Básico", "7° Básico", "8° Básico", "1° Medio", "2° Medio", "3° Medio", "4° Medio" });
            currentY += gap;

            this.Controls.Add(new Label { Text = "Rol:", Location = new Point(labelX, currentY) });
            cmbRol = new ComboBox { Location = new Point(inputX, currentY), Size = new Size(200, 25), DropDownStyle = ComboBoxStyle.DropDownList };
            cmbRol.Items.AddRange(new string[] { "Estudiante", "Docente", "Funcionario", "Apoderado" });
            currentY += gap;

            this.Controls.Add(new Label { Text = "Email:", Location = new Point(labelX, currentY) });
            txtEmail = new TextBox { Location = new Point(inputX, currentY), Size = new Size(200, 25) };
            currentY += gap;

            this.Controls.Add(new Label { Text = "Teléfono:", Location = new Point(labelX, currentY) });
            txtTelefono = new TextBox { Location = new Point(inputX, currentY), Size = new Size(200, 25) };
            currentY += gap;

            btnGuardar = new Button { Text = "Guardar", Location = new Point(150, 410), Size = new Size(100, 35), BackColor = Color.FromArgb(0, 120, 215), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnGuardar.Click += BtnGuardar_Click;
            this.Controls.Add(btnGuardar);
        }

        private void CargarDatos()
        {
            if (_socio == null) return;
            txtRut.Text = _socio.RUT;
            txtNombre.Text = _socio.Nombre;
            txtApellidos.Text = _socio.Apellidos;
            cmbNivel.SelectedItem = _socio.Nivel;
            cmbCurso.Text = _socio.Curso;
            cmbRol.SelectedItem = _socio.Rol;
            txtEmail.Text = _socio.Email;
            txtTelefono.Text = _socio.Telefono;
        }

        private async void BtnGuardar_Click(object? sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(txtRut.Text) && !ValidadorRut.ValidarRut(txtRut.Text))
            {
                MessageBox.Show("RUT inválido.");
                return;
            }

            if (string.IsNullOrWhiteSpace(txtNombre.Text) || string.IsNullOrWhiteSpace(txtApellidos.Text))
            {
                MessageBox.Show("Nombre y Apellidos son obligatorios.");
                return;
            }

            using var conexion = _db.ObtenerConexion();
            conexion.Open();

            string sql;
            var param = new {
                RUT = ValidadorRut.FormatearRut(txtRut.Text),
                Nombre = txtNombre.Text,
                Apellidos = txtApellidos.Text,
                Nivel = cmbNivel.SelectedItem?.ToString(),
                Curso = cmbCurso.Text,
                Rol = cmbRol.SelectedItem?.ToString(),
                Email = txtEmail.Text,
                Telefono = txtTelefono.Text,
                Id = _socio?.Id ?? 0
            };

            if (_socio == null)
            {
                sql = "INSERT INTO Socios (RUT, Nombre, Apellidos, Nivel, Curso, Rol, Email, Telefono) VALUES (@RUT, @Nombre, @Apellidos, @Nivel, @Curso, @Rol, @Email, @Telefono)";
            }
            else
            {
                sql = "UPDATE Socios SET RUT=@RUT, Nombre=@Nombre, Apellidos=@Apellidos, Nivel=@Nivel, Curso=@Curso, Rol=@Rol, Email=@Email, Telefono=@Telefono WHERE Id=@Id";
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
