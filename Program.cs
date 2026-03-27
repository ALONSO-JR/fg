using System;
using System.IO;
using System.Windows.Forms;
using SistemaGestionCRA.Datos;
using SistemaGestionCRA.Vistas;
using Dapper;

namespace SistemaGestionCRA
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();

            string appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string appFolder = Path.Combine(appData, "SistemaCRA");
            if (!Directory.Exists(appFolder)) Directory.CreateDirectory(appFolder);

            string dbPath = Path.Combine(appFolder, "cra_database.db");
            var dbHelper = new BaseDatosHelper(dbPath);
            dbHelper.InicializarBaseDatos();

            // Verificar si hay configuración inicial
            using var conexion = dbHelper.ObtenerConexion();
            conexion.Open();
            var instCount = (long)new Microsoft.Data.Sqlite.SqliteCommand("SELECT COUNT(1) FROM Institucion", (Microsoft.Data.Sqlite.SqliteConnection)conexion).ExecuteScalar();

            if (instCount == 0)
            {
                using var formConfig = new FormConfigInicial(dbHelper);
                if (formConfig.ShowDialog() != DialogResult.OK)
                {
                    return;
                }
            }

            // Iniciar Sesión
            using var formLogin = new FormLogin(dbHelper);
            if (formLogin.ShowDialog() == DialogResult.OK && formLogin.UsuarioLogueado != null)
            {
                Application.Run(new FormPrincipal(dbHelper, formLogin.UsuarioLogueado));
            }
        }
    }
}
