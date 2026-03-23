using System;
using System.IO;
using System.Windows.Forms;
using SistemaGestionCRA.Datos;
using Microsoft.Data.Sqlite;

namespace SistemaGestionCRA.Logica
{
    public class RespaldoLogica
    {
        private readonly string _dbPath;

        public RespaldoLogica(string dbPath)
        {
            _dbPath = dbPath;
        }

        public string RealizarRespaldo(string rutaDestino)
        {
            try {
                if (!File.Exists(_dbPath)) return "Base de datos no encontrada.";

                string fecha = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                string nombreArchivo = $"Respaldo_CRA_{fecha}.db";
                string fullPath = Path.Combine(rutaDestino, nombreArchivo);

                File.Copy(_dbPath, fullPath, true);
                return "OK|" + fullPath;
            } catch (Exception ex) {
                return "Error: " + ex.Message;
            }
        }

        public string RestaurarRespaldo(string rutaRespaldo)
        {
            try {
                if (!File.Exists(rutaRespaldo)) return "Archivo de respaldo no encontrado.";

                // Detener conexiones sería ideal, pero SQLite permite sobreescribir si no está bloqueado
                File.Copy(rutaRespaldo, _dbPath, true);
                return "OK";
            } catch (Exception ex) {
                return "Error: " + ex.Message;
            }
        }
    }
}
