using System;
using System.Collections.Generic;
using Microsoft.Data.Sqlite;
using Dapper;

namespace SistemaGestionCRA.Datos
{
    public class BaseDatosHelper
    {
        private readonly string _cadenaConexion;

        public BaseDatosHelper(string rutaDb)
        {
            _cadenaConexion = $"Data Source={rutaDb}";
        }

        public void InicializarBaseDatos()
        {
            using var conexion = new SqliteConnection(_cadenaConexion);
            conexion.Open();

            // Habilitar modo WAL para mejor concurrencia en red local
            using var comandoWal = new SqliteCommand("PRAGMA journal_mode=WAL;", conexion);
            comandoWal.ExecuteNonQuery();

            var tablas = new List<string>
            {
                // Institución
                @"CREATE TABLE IF NOT EXISTS Institucion (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Nombre TEXT NOT NULL,
                    RBD TEXT,
                    Direccion TEXT,
                    Telefono TEXT,
                    Email TEXT
                );",

                // Parámetros
                @"CREATE TABLE IF NOT EXISTS Parametros (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    DiasPrestamo INTEGER DEFAULT 7,
                    MaxLibrosPorSocio INTEGER DEFAULT 3,
                    MaxRenovaciones INTEGER DEFAULT 1,
                    MultaDiaria REAL DEFAULT 0
                );",

                // Usuarios del sistema
                @"CREATE TABLE IF NOT EXISTS Usuarios (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    NombreUsuario TEXT UNIQUE NOT NULL,
                    PasswordHash TEXT NOT NULL,
                    Rol TEXT NOT NULL, -- Administrador, Encargado, Consulta
                    NombreCompleto TEXT
                );",

                // Socios
                @"CREATE TABLE IF NOT EXISTS Socios (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    RUT TEXT UNIQUE,
                    Nombre TEXT NOT NULL,
                    Apellidos TEXT NOT NULL,
                    Curso TEXT,
                    Nivel TEXT, -- Pre-básica, Básica, Media
                    Rol TEXT, -- Estudiante, Docente, Funcionario, Apoderado
                    Telefono TEXT,
                    Email TEXT,
                    Estado TEXT DEFAULT 'Activo' -- Activo, Inactivo, Sancionado
                );",

                // Catálogo (Ejemplares)
                @"CREATE TABLE IF NOT EXISTS Ejemplares (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    CodigoBarras TEXT UNIQUE NOT NULL,
                    ISBN TEXT,
                    Titulo TEXT NOT NULL,
                    Autor TEXT,
                    Editorial TEXT,
                    Anio INTEGER,
                    Tipo TEXT, -- Libro, Revista, Audiovisual, etc.
                    ClasificacionDewey TEXT,
                    Cutter TEXT,
                    Ubicacion TEXT,
                    Estado TEXT DEFAULT 'Disponible' -- Disponible, Prestado, Perdido, Danado, En Reparacion
                );",

                // Préstamos
                @"CREATE TABLE IF NOT EXISTS Prestamos (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    SocioId INTEGER NOT NULL,
                    EjemplarId INTEGER NOT NULL,
                    FechaPrestamo DATETIME DEFAULT CURRENT_TIMESTAMP,
                    FechaVencimiento DATETIME NOT NULL,
                    FechaDevolucion DATETIME,
                    Estado TEXT DEFAULT 'Pendiente', -- Pendiente, Devuelto, Renovado
                    RenovacionesRealizadas INTEGER DEFAULT 0,
                    FOREIGN KEY (SocioId) REFERENCES Socios(Id),
                    FOREIGN KEY (EjemplarId) REFERENCES Ejemplares(Id)
                );",

                // Sanciones (Multas)
                @"CREATE TABLE IF NOT EXISTS Sanciones (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    SocioId INTEGER NOT NULL,
                    PrestamoId INTEGER,
                    FechaInicio DATETIME DEFAULT CURRENT_TIMESTAMP,
                    FechaFin DATETIME,
                    Monto REAL DEFAULT 0,
                    Descripcion TEXT,
                    Estado TEXT DEFAULT 'Activa', -- Activa, Pagada, Anulada
                    FOREIGN KEY (SocioId) REFERENCES Socios(Id),
                    FOREIGN KEY (PrestamoId) REFERENCES Prestamos(Id)
                );",

                // Reservas
                @"CREATE TABLE IF NOT EXISTS Reservas (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    SocioId INTEGER NOT NULL,
                    EjemplarId INTEGER NOT NULL,
                    FechaReserva DATETIME DEFAULT CURRENT_TIMESTAMP,
                    Estado TEXT DEFAULT 'Activa', -- Activa, Completada, Cancelada
                    FOREIGN KEY (SocioId) REFERENCES Socios(Id),
                    FOREIGN KEY (EjemplarId) REFERENCES Ejemplares(Id)
                );",

                // Bitácora
                @"CREATE TABLE IF NOT EXISTS Bitacora (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Fecha DATETIME DEFAULT CURRENT_TIMESTAMP,
                    UsuarioId INTEGER,
                    Accion TEXT NOT NULL,
                    Detalle TEXT,
                    FOREIGN KEY (UsuarioId) REFERENCES Usuarios(Id)
                );"
            };

            foreach (var sql in tablas)
            {
                using var comando = new SqliteCommand(sql, conexion);
                comando.ExecuteNonQuery();
            }

            // Insertar usuario admin por defecto si no existe
            const string sqlCheckAdmin = "SELECT COUNT(1) FROM Usuarios WHERE NombreUsuario = 'admin'";
            long count = (long)new SqliteCommand(sqlCheckAdmin, conexion).ExecuteScalar();
            if (count == 0)
            {
                // Password 'admin' hasheado
                string hash = Logica.SeguridaLogica.GenerarHashPassword("admin");
                const string sqlInsertAdmin = "INSERT INTO Usuarios (NombreUsuario, PasswordHash, Rol, NombreCompleto) VALUES ('admin', @Hash, 'Administrador', 'Administrador Sistema')";
                using var comandoAdmin = new SqliteCommand(sqlInsertAdmin, conexion);
                comandoAdmin.Parameters.AddWithValue("@Hash", hash);
                comandoAdmin.ExecuteNonQuery();
            }
        }

        public SqliteConnection ObtenerConexion()
        {
            return new SqliteConnection(_cadenaConexion);
        }
    }
}
