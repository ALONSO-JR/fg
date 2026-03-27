using System;
using System.Drawing;
using System.Windows.Forms;
using SistemaGestionCRA.Datos;
using SistemaGestionCRA.Modelos;
using SistemaGestionCRA.Logica;
using Dapper;

namespace SistemaGestionCRA.Vistas
{
    public partial class FormLogin : Form
    {
        private readonly BaseDatosHelper _db;
        private TextBox txtUsuario;
        private TextBox txtPassword;
        private Button btnLogin;
        public Usuario? UsuarioLogueado { get; private set; }

        public FormLogin(BaseDatosHelper db)
        {
            _db = db;
            InitializeComponentManual();
        }

        private void InitializeComponentManual()
        {
            this.Text = "Iniciar Sesión - Sistema CRA";
            this.Size = new Size(350, 400);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;

            var panel = new Panel { Dock = DockStyle.Fill, BackColor = Color.White };

            var lblTitulo = new Label { Text = "SISTEMA CRA", Font = new Font("Segoe UI", 16, FontStyle.Bold), Location = new Point(50, 40), Size = new Size(250, 40), ForeColor = Color.FromArgb(0, 120, 215), TextAlign = ContentAlignment.MiddleCenter };

            var lblUser = new Label { Text = "Usuario:", Location = new Point(50, 100), Size = new Size(250, 20) };
            txtUsuario = new TextBox { Location = new Point(50, 125), Size = new Size(250, 30), Font = new Font("Segoe UI", 12) };

            var lblPass = new Label { Text = "Contraseña:", Location = new Point(50, 170), Size = new Size(250, 20) };
            txtPassword = new TextBox { Location = new Point(50, 195), Size = new Size(250, 30), Font = new Font("Segoe UI", 12), PasswordChar = '●' };

            btnLogin = new Button { Text = "INGRESAR", Location = new Point(50, 260), Size = new Size(250, 45), BackColor = Color.FromArgb(0, 120, 215), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 12, FontStyle.Bold) };
            btnLogin.Click += BtnLogin_Click;

            this.Controls.Add(panel);
            panel.Controls.AddRange(new Control[] { lblTitulo, lblUser, txtUsuario, lblPass, txtPassword, btnLogin });

            this.AcceptButton = btnLogin;
        }

        private void BtnLogin_Click(object? sender, EventArgs e)
        {
            string user = txtUsuario.Text.Trim();
            string pass = txtPassword.Text;

            using var conexion = _db.ObtenerConexion();
            conexion.Open();

            var u = conexion.QueryFirstOrDefault<Usuario>("SELECT * FROM Usuarios WHERE NombreUsuario = @Nombre", new { Nombre = user });

            if (u != null && SeguridaLogica.VerificarPassword(pass, u.PasswordHash ?? ""))
            {
                UsuarioLogueado = u;
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            else
            {
                MessageBox.Show("Usuario o contraseña incorrectos.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
