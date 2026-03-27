using System;
using System.Collections.Generic;
using System.IO;
using DotNetDBF;
using SistemaGestionCRA.Datos;
using SistemaGestionCRA.Modelos;
using Dapper;

namespace SistemaGestionCRA.Logica
{
    public class MigracionDbfLogica
    {
        private readonly BaseDatosHelper _db;

        public MigracionDbfLogica(BaseDatosHelper db)
        {
            _db = db;
        }

        public List<string> ObtenerCamposDbf(string rutaArchivo)
        {
            var campos = new List<string>();
            try {
                using var stream = File.Open(rutaArchivo, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                using var reader = new DBFReader(stream);
                for (int i = 0; i < reader.Fields.Length; i++) {
                    campos.Add(reader.Fields[i].Name);
                }
            } catch (Exception) { /* Manejar error */ }
            return campos;
        }

        public (int Procesados, int Errores, string Log) ImportarSocios(string rutaArchivo, Dictionary<string, string> mapeo)
        {
            int procesados = 0;
            int errores = 0;
            var log = new System.Text.StringBuilder();

            try {
                using var stream = File.Open(rutaArchivo, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                using var reader = new DBFReader(stream);
                object[]? row;

                using var conexion = _db.ObtenerConexion();
                conexion.Open();

                while ((row = reader.NextRecord()) != null) {
                    try {
                        var socio = new Socio {
                            RUT = GetValue(row, reader, mapeo, "RUT"),
                            Nombre = GetValue(row, reader, mapeo, "Nombre") ?? "Sin Nombre",
                            Apellidos = GetValue(row, reader, mapeo, "Apellidos") ?? "Sin Apellido",
                            Curso = GetValue(row, reader, mapeo, "Curso"),
                            Rol = GetValue(row, reader, mapeo, "Rol") ?? "Estudiante",
                            Estado = "Activo"
                        };

                        conexion.Execute(@"INSERT OR IGNORE INTO Socios (RUT, Nombre, Apellidos, Curso, Rol, Estado)
                                           VALUES (@RUT, @Nombre, @Apellidos, @Curso, @Rol, @Estado)", socio);
                        procesados++;
                    } catch (Exception ex) {
                        errores++;
                        log.AppendLine($"Error en registro {procesados + errores}: {ex.Message}");
                    }
                }
            } catch (Exception ex) {
                log.AppendLine("Error general: " + ex.Message);
            }

            return (procesados, errores, log.ToString());
        }

        private string? GetValue(object[] row, DBFReader reader, Dictionary<string, string> mapeo, string campoDestino)
        {
            if (mapeo.TryGetValue(campoDestino, out string? dbfFieldName)) {
                for (int i = 0; i < reader.Fields.Length; i++) {
                    if (reader.Fields[i].Name.Equals(dbfFieldName, StringComparison.OrdinalIgnoreCase)) {
                        return row[i]?.ToString()?.Trim();
                    }
                }
            }
            return null;
        }
    }
}
