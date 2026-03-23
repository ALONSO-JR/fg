using System;
using System.Drawing;
using System.Windows.Forms;
using SistemaGestionCRA.Datos;
using SistemaGestionCRA.Modelos;
using Dapper;

namespace SistemaGestionCRA.Vistas
{
    public partial class FormConfigInicial : Form
    {
        private readonly BaseDatosHelper _db;
        private TextBox txtNombreInst;
        private TextBox txtRBD;
        private NumericUpDown numDiasPrestamo;
        private NumericUpDown numMaxLibros;
        private Button btnGuardar;

        public FormConfigInicial(BaseDatosHelper db)
        {
            _db = db;
            InitializeComponentManual();
        }

        private void InitializeComponentManual()
        {
            this.Text = "Configuración Inicial - Sistema CRA";
            this.Size = new Size(400, 450);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;

            var lblTitulo = new Label { Text = "Configuración Inicial de la Institución", Font = new Font("Segoe UI", 12, FontStyle.Bold), Location = new Point(20, 20), Size = new Size(350, 30) };

            var lblNombre = new Label { Text = "Nombre de la Institución:", Location = new Point(20, 70), Size = new Size(150, 20) };
            txtNombreInst = new TextBox { Location = new Point(20, 95), Size = new Size(340, 25) };

            var lblRBD = new Label { Text = "RBD (Rol Base de Datos):", Location = new Point(20, 130), Size = new Size(150, 20) };
            txtRBD = new TextBox { Location = new Point(20, 155), Size = new Size(100, 25), MaxLength = 8 };

            var lblParams = new Label { Text = "Parámetros de Préstamo", Font = new Font("Segoe UI", 10, FontStyle.Bold), Location = new Point(20, 200), Size = new Size(200, 25) };

            var lblDias = new Label { Text = "Días de préstamo:", Location = new Point(20, 235), Size = new Size(150, 20) };
            numDiasPrestamo = new NumericUpDown { Location = new Point(180, 235), Size = new Size(60, 25), Minimum = 1, Value = 7 };

            var lblMax = new Label { Text = "Máximo de libros p/socio:", Location = new Point(20, 275), Size = new Size(150, 20) };
            numMaxLibros = new NumericUpDown { Location = new Point(180, 275), Size = new Size(60, 25), Minimum = 1, Value = 3 };

            btnGuardar = new Button { Text = "Guardar y Continuar", Location = new Point(120, 350), Size = new Size(150, 40), BackColor = Color.FromArgb(0, 120, 215), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnGuardar.Click += BtnGuardar_Click;

            this.Controls.AddRange(new Control[] { lblTitulo, lblNombre, txtNombreInst, lblRBD, txtRBD, lblParams, lblDias, numDiasPrestamo, lblMax, numMaxLibros, btnGuardar });
        }

        private async void BtnGuardar_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtNombreInst.Text))
            {
                MessageBox.Show("El nombre de la institución es obligatorio.");
                return;
            }

            using var conexion = _db.ObtenerConexion();
            conexion.Open();

            using var transaccion = conexion.BeginTransaction();
            try
            {
                await conexion.ExecuteAsync("INSERT INTO Institucion (Nombre, RBD) VALUES (@Nombre, @RBD)", new { Nombre = txtNombreInst.Text, RBD = txtRBD.Text }, transaccion);

                string[] tipos = { "Libro", "Revista", "Manual", "Audiovisual", "Recurso Pedagógico" };
                foreach (var tipo in tipos) {
                    await conexion.ExecuteAsync("INSERT INTO ReglasPrestamo (TipoMaterial, DiasPrestamo, MaxLibros) VALUES (@Tipo, @Dias, @Max)",
                                                 new { Tipo = tipo, Dias = (int)numDiasPrestamo.Value, Max = (int)numMaxLibros.Value }, transaccion);
                }

                transaccion.Commit();
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                transaccion.Rollback();
                MessageBox.Show("Error al guardar la configuración: " + ex.Message);
            }
        }
    }
}
