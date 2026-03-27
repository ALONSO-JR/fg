using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using SistemaGestionCRA.Datos;
using SistemaGestionCRA.Modelos;
using Dapper;

namespace SistemaGestionCRA.Logica
{
    public class CirculacionLogica
    {
        private readonly BaseDatosHelper _db;

        public CirculacionLogica(BaseDatosHelper db)
        {
            _db = db;
        }

        public async Task<string> Prestar(int socioId, int ejemplarId)
        {
            using var conexion = _db.ObtenerConexion();
            conexion.Open();

            // 1. Validar socio
            var socio = await conexion.QueryFirstOrDefaultAsync<Socio>("SELECT * FROM Socios WHERE Id = @Id", new { Id = socioId });
            if (socio == null) return "El socio no existe.";
            if (socio.Estado != "Activo") return $"El socio no puede realizar préstamos. Estado actual: {socio.Estado}.";
            if (socio.BloqueadoHasta.HasValue && socio.BloqueadoHasta.Value > DateTime.Now)
                return $"El socio está bloqueado hasta el {socio.BloqueadoHasta.Value:dd/MM/yyyy} por devoluciones tardías.";

            // 2. Validar ejemplar
            var ejemplar = await conexion.QueryFirstOrDefaultAsync<Ejemplar>("SELECT * FROM Ejemplares WHERE Id = @Id", new { Id = ejemplarId });
            if (ejemplar == null) return "El ejemplar no existe.";
            if (ejemplar.Estado != "Disponible") return $"El ejemplar no está disponible. Estado: {ejemplar.Estado}.";

            // 3. Obtener Reglas por Tipo de Material
            var regla = await conexion.QueryFirstOrDefaultAsync<ReglaPrestamo>("SELECT * FROM ReglasPrestamo WHERE TipoMaterial = @Tipo", new { Tipo = ejemplar.Tipo })
                        ?? new ReglaPrestamo { DiasPrestamo = 7, MaxLibros = 3, MaxRenovaciones = 1 };

            // 4. Validar límites de préstamos
            int prestamosActivos = await conexion.QuerySingleAsync<int>("SELECT COUNT(1) FROM Prestamos WHERE SocioId = @SocioId AND Estado = 'Pendiente'", new { SocioId = socioId });

            if (prestamosActivos >= regla.MaxLibros)
            {
                return $"El socio ya tiene el máximo permitido de préstamos para este tipo de material ({regla.MaxLibros}).";
            }

            // 4. Registrar préstamo
            using var transaccion = conexion.BeginTransaction();
            try
            {
                var fechaPrestamo = DateTime.Now;
                var fechaVencimiento = fechaPrestamo.AddDays(regla.DiasPrestamo);

                string sqlPrestamo = @"INSERT INTO Prestamos (SocioId, EjemplarId, FechaPrestamo, FechaVencimiento, Estado)
                                       VALUES (@SocioId, @EjemplarId, @FechaPrestamo, @FechaVencimiento, 'Pendiente');";
                await conexion.ExecuteAsync(sqlPrestamo, new { SocioId = socioId, EjemplarId = ejemplarId, FechaPrestamo = fechaPrestamo, FechaVencimiento = fechaVencimiento }, transaccion);

                string sqlUpdateEjemplar = "UPDATE Ejemplares SET Estado = 'Prestado' WHERE Id = @Id;";
                await conexion.ExecuteAsync(sqlUpdateEjemplar, new { Id = ejemplarId }, transaccion);

                transaccion.Commit();
                return "OK";
            }
            catch (Exception ex)
            {
                transaccion.Rollback();
                return "Error al registrar el préstamo: " + ex.Message;
            }
        }

        public async Task<string> Devolver(int ejemplarId)
        {
            using var conexion = _db.ObtenerConexion();
            conexion.Open();

            var prestamo = await conexion.QueryFirstOrDefaultAsync<Prestamo>("SELECT p.*, e.Tipo FROM Prestamos p JOIN Ejemplares e ON p.EjemplarId = e.Id WHERE p.EjemplarId = @EjemplarId AND p.Estado = 'Pendiente'", new { EjemplarId = ejemplarId });
            if (prestamo == null) return "No se encontró un préstamo pendiente para este ejemplar.";

            using var transaccion = conexion.BeginTransaction();
            try
            {
                DateTime fechaDevolucion = DateTime.Now;

                // Actualizar préstamo
                await conexion.ExecuteAsync("UPDATE Prestamos SET FechaDevolucion = @Fecha, Estado = 'Devuelto' WHERE Id = @Id", new { Fecha = fechaDevolucion, Id = prestamo.Id }, transaccion);

                // Actualizar ejemplar
                await conexion.ExecuteAsync("UPDATE Ejemplares SET Estado = 'Disponible' WHERE Id = @Id", new { Id = ejemplarId }, transaccion);

                // Calcular bloqueo si aplica
                if (fechaDevolucion.Date > prestamo.FechaVencimiento.Date)
                {
                    // Obtener tipo de ejemplar (guardado dinámicamente o consultando de nuevo)
                    var ejemplar = await conexion.QueryFirstOrDefaultAsync<Ejemplar>("SELECT * FROM Ejemplares WHERE Id = @Id", new { Id = ejemplarId }, transaccion);
                    var regla = await conexion.QueryFirstOrDefaultAsync<ReglaPrestamo>("SELECT * FROM ReglasPrestamo WHERE TipoMaterial = @Tipo", new { Tipo = ejemplar.Tipo }, transaccion)
                                ?? new ReglaPrestamo { DiasSancionPorAtraso = 1 };

                    if (regla.DiasSancionPorAtraso > 0)
                    {
                        int diasAtraso = (fechaDevolucion.Date - prestamo.FechaVencimiento.Date).Days;
                        int diasBloqueo = diasAtraso * regla.DiasSancionPorAtraso;
                        DateTime nuevaFechaBloqueo = DateTime.Now.AddDays(diasBloqueo);

                        await conexion.ExecuteAsync(@"UPDATE Socios SET BloqueadoHasta = @Bloqueo, Estado = 'Sancionado' WHERE Id = @SocioId",
                                                     new { Bloqueo = nuevaFechaBloqueo, SocioId = prestamo.SocioId }, transaccion);

                        await conexion.ExecuteAsync(@"INSERT INTO Sanciones (SocioId, PrestamoId, FechaInicio, FechaFin, Descripcion, Estado)
                                                     VALUES (@SocioId, @PrestamoId, @Inicio, @Fin, @Desc, 'Activa')",
                                                     new { SocioId = prestamo.SocioId, PrestamoId = prestamo.Id, Inicio = DateTime.Now, Fin = nuevaFechaBloqueo, Desc = $"Bloqueo de {diasBloqueo} días por atraso de {diasAtraso} días." }, transaccion);
                    }
                }

                transaccion.Commit();
                return "OK";
            }
            catch (Exception ex)
            {
                transaccion.Rollback();
                return "Error al registrar la devolución: " + ex.Message;
            }
        }
    }

    public static class SeguridaLogica
    {
        public static string GenerarHashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(password);
            var hash = sha256.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }

        public static bool VerificarPassword(string password, string hash)
        {
            return GenerarHashPassword(password) == hash;
        }
    }
}
