using System;
using System.Drawing;
using System.Windows.Forms;
using SistemaGestionCRA.Datos;
using SistemaGestionCRA.Modelos;
using Dapper;

namespace SistemaGestionCRA.Vistas
{
    public partial class FormPrincipal : Form
    {
        private readonly BaseDatosHelper _db;
        private readonly Usuario _usuario;
        private Panel panelMenu;
        private Panel panelContenido;
        private Label lblUsuario;
        private Label lblEstablecimiento;

        public FormPrincipal(BaseDatosHelper db, Usuario usuario)
        {
            _db = db;
            _usuario = usuario;
            InitializeComponentManual();
            CargarDatosInstitucion();
        }

        private void InitializeComponentManual()
        {
            this.Text = "Sistema de Gestión CRA (On-Premise) - Chile";
            this.WindowState = FormWindowState.Maximized;
            this.MinimumSize = new Size(1024, 768);
            this.Font = new Font("Segoe UI", 10);
            this.BackColor = Color.FromArgb(243, 243, 243);

            // Panel Superior
            var panelTop = new Panel { Dock = DockStyle.Top, Height = 60, BackColor = Color.FromArgb(0, 120, 215) };
            lblEstablecimiento = new Label { Text = "Institución: Carga en curso...", ForeColor = Color.White, Font = new Font("Segoe UI", 12, FontStyle.Bold), Location = new Point(20, 15), AutoSize = true };
            lblUsuario = new Label { Text = $"Usuario: {_usuario.NombreCompleto} ({_usuario.Rol})", ForeColor = Color.White, Location = new Point(0, 15), AutoSize = true, Anchor = AnchorStyles.Right };
            panelTop.Controls.AddRange(new Control[] { lblEstablecimiento, lblUsuario });

            // Reposicionar lblUsuario manualmente para simular anclaje derecho
            lblUsuario.Left = this.Width - lblUsuario.Width - 150;

            // Panel Lateral de Menú
            panelMenu = new Panel { Dock = DockStyle.Left, Width = 220, BackColor = Color.White };

            var btnDashboard = CrearBotonMenu("🏠 Dashboard", 0);
            var btnCirculacion = CrearBotonMenu("🔄 Circulación", 60);
            var btnSocios = CrearBotonMenu("👥 Socios", 120);
            var btnCatalogo = CrearBotonMenu("📚 Catálogo", 180);
            var btnInventario = CrearBotonMenu("📦 Inventario", 240);
            var btnReportes = CrearBotonMenu("📊 Reportes", 300);
            var btnConfig = CrearBotonMenu("⚙️ Configuración", 360);
            var btnSalir = CrearBotonMenu("🚪 Salir", 420);

            panelMenu.Controls.AddRange(new Control[] { btnDashboard, btnCirculacion, btnSocios, btnCatalogo, btnInventario, btnReportes, btnConfig, btnSalir });

            // Panel de Contenido
            panelContenido = new Panel { Dock = DockStyle.Fill, BackColor = Color.White, Margin = new Padding(20), Padding = new Padding(10) };

            this.Controls.Add(panelContenido);
            this.Controls.Add(panelMenu);
            this.Controls.Add(panelTop);
        }

        private Button CrearBotonMenu(string texto, int top)
        {
            var btn = new Button
            {
                Text = texto,
                TextAlign = ContentAlignment.MiddleLeft,
                Location = new Point(0, top),
                Size = new Size(220, 60),
                FlatStyle = FlatStyle.Flat,
                FlatAppearance = { BorderSize = 0, MouseOverBackColor = Color.FromArgb(220, 235, 252) },
                Font = new Font("Segoe UI", 11),
                Padding = new Padding(20, 0, 0, 0),
                BackColor = Color.White,
                ForeColor = Color.FromArgb(64, 64, 64)
            };
            btn.Click += BotonMenu_Click;
            return btn;
        }

        private void BotonMenu_Click(object? sender, EventArgs e)
        {
            if (sender is Button btn)
            {
                // Limpiar contenido actual
                panelContenido.Controls.Clear();

                if (btn.Text.Contains("Socios"))
                {
                    panelContenido.Controls.Add(new PanelSocios(_db));
                }
                else if (btn.Text.Contains("Catálogo"))
                {
                    panelContenido.Controls.Add(new PanelCatalogo(_db));
                }
                else if (btn.Text.Contains("Circulación"))
                {
                    panelContenido.Controls.Add(new PanelCirculacion(_db));
                }
                else if (btn.Text.Contains("Inventario"))
                {
                    panelContenido.Controls.Add(new PanelInventario(_db));
                }
                else if (btn.Text.Contains("Reportes"))
                {
                    panelContenido.Controls.Add(new PanelReportes(_db));
                }
                else if (btn.Text.Contains("Configuración"))
                {
                    panelContenido.Controls.Add(new PanelConfiguracion(_db));
                }
                else if (btn.Text.Contains("Salir"))
                {
                    Application.Exit();
                }
                else
                {
                    var lbl = new Label { Text = $"Módulo en construcción: {btn.Text}", Font = new Font("Segoe UI", 14), Location = new Point(50, 50), AutoSize = true };
                    panelContenido.Controls.Add(lbl);
                }
            }
        }

        private void CargarDatosInstitucion()
        {
            using var conexion = _db.ObtenerConexion();
            conexion.Open();
            var inst = conexion.QueryFirstOrDefault<Institucion>("SELECT * FROM Institucion LIMIT 1");
            if (inst != null)
            {
                lblEstablecimiento.Text = $"Establecimiento: {inst.Nombre} (RBD: {inst.RBD})";
            }
        }
    }
}
